using Grpc.Core;
using Http;
using MySubnet.BlockChain;
using MySubnet.JsonRpc;
using MySubnet.Shared;

namespace MySubnet.Avalanche;

public class HttpServer : HTTP.HTTPBase
{
    private readonly AppSenderClient _appSender;
    private readonly BlockChain.BlockChain _blockchain;
    private readonly GlobalSettings _globalSettings;
    private readonly ILogger _logger;

    public HttpServer(ILogger<VmServer> logger, GlobalSettings globalSettings, BlockChain.BlockChain blockChain,
        AppSenderClient appSender)
    {
        _logger = logger;
        _globalSettings = globalSettings;
        _blockchain = blockChain;
        _appSender = appSender;
    }

    public override Task<HandleSimpleHTTPResponse> HandleSimple(HandleSimpleHTTPRequest request,
        ServerCallContext context)
    {
        try
        {
            if (request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                var jsonBody = request.Body.ToStringUtf8();
                _logger.LogInformation("Http request body : {jsonBody}", jsonBody);
                var rpcRequest = jsonBody.DeserializeJson<JsonRpcRequest>();
                if (rpcRequest != null) return HandleJsonRpcRequest(rpcRequest);
            }
        }
        catch (Exception e)
        {
            return Task.FromResult(BuildResponse(null, null,
                new JsonRpcError("ERROR", "Error : " + e.Message, null)));
        }

        return Task.FromResult(BuildResponse(null, null,
            new JsonRpcError("INVALID_REQUEST", "Invalid Json-RPC request", null)));
    }

    private async Task<HandleSimpleHTTPResponse> HandleJsonRpcRequest(JsonRpcRequest rpcRequest)
    {
        return rpcRequest.Method switch
        {
            "Transfer" => await Transfer(rpcRequest),
            "LatestBlock" => GetLatestBlock(rpcRequest),
            "BlockchainState" => GetBlockchainState(rpcRequest),
            _ => BuildResponse(rpcRequest, null,
                new JsonRpcError("NOT_IMPLEMENTED", $"Method {rpcRequest.Method} is not implemented", null))
        };
    }

    private HandleSimpleHTTPResponse GetBlockchainState(JsonRpcRequest rpcRequest)
    {
        var data = new
        {
            FinalBlocks = _blockchain.FinalBlocks.Select(x => x.Value).ToArray(),
            _blockchain.LatestBlock,
            Pending = _blockchain.PendingBlocks.Select(x => x.Value).ToArray(),
            _blockchain.PreferredBlock,
            _blockchain.State,
            Pool = _blockchain.TransactionPool.Select(x => x.Value).ToArray()
        };
        return BuildResponse(rpcRequest, data, null);
    }


    private HandleSimpleHTTPResponse GetLatestBlock(JsonRpcRequest rpcRequest)
    {
        return BuildResponse(rpcRequest, _blockchain.LatestBlock, null);
    }

    private async Task<HandleSimpleHTTPResponse> Transfer(JsonRpcRequest rpcRequest)
    {
        var transaction = rpcRequest.DeserializeParameters<Transaction>();
        if (transaction == null || transaction.From == null || transaction.To == null)
            return BuildResponse(rpcRequest, null,
                new JsonRpcError("INVALID_TRANSACTION", "Unable to parse transaction " + rpcRequest.Parameters, null));
        transaction = transaction with
        {
            Hash = transaction.HashObject()
        };
        _blockchain.AddPendingTransaction(transaction);
        await _appSender.GossipTransaction(transaction);
        return BuildResponse(rpcRequest, transaction, null);
    }

    private HandleSimpleHTTPResponse BuildResponse(JsonRpcRequest? request, object? result, JsonRpcError? error)
    {
        var response = new JsonRpcResponse("2.0", request?.Id, error, result);
        return new HandleSimpleHTTPResponse
        {
            Body = response.SerializeJsonByteString(),
            Code = response.Error == null ? 200 : 400
        };
    }
}