using System.Reflection;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MySubnet.BlockChain;
using MySubnet.Shared;
using Vm;
using Status = Vm.Status;

namespace MySubnet.Avalanche;

public class VmServer : VM.VMBase
{
    private readonly BlockChain.BlockChain _blockchain;
    private readonly GlobalSettings _globalSettings;
    private readonly ILogger _logger;

    public VmServer(ILogger<VmServer> logger, GlobalSettings globalSettings, BlockChain.BlockChain blockChain)
    {
        _logger = logger;
        _globalSettings = globalSettings;
        _blockchain = blockChain;
    }

    public override Task<VersionResponse> Version(Empty request, ServerCallContext context)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "UndefinedVersion";
        _logger.LogInformation("VmServer.Version called => {version}", version);
        return Task.FromResult(new VersionResponse
        {
            Version = version
        });
    }

    public override Task<CreateStaticHandlersResponse> CreateStaticHandlers(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new CreateStaticHandlersResponse());
    }

    public override Task<Empty> Shutdown(Empty request, ServerCallContext context)
    {
        _globalSettings.TokenSource.Cancel();
        return Task.FromResult(new Empty());
    }

    public override Task<InitializeResponse> Initialize(InitializeRequest request, ServerCallContext context)
    {
        var genesisBlock = request.GenesisBytes.DeserializeJsonByteString<Block>();
        if (genesisBlock == null) return Task.FromResult(new InitializeResponse());
        if (request.ServerAddr != _globalSettings.AvalancheRpcUrl) _globalSettings.AvalancheRpcUrl = request.ServerAddr;
        _blockchain.AddFinalBlock(genesisBlock);
        return Task.FromResult(new InitializeResponse
        {
            Bytes = request.GenesisBytes,
            Height = genesisBlock.Height,
            LastAcceptedId = genesisBlock.Hash.HexStringToByteString(),
            LastAcceptedParentId = genesisBlock.ParentHash.HexStringToByteString(),
            Timestamp = genesisBlock.Timestamp.LongToTimestamp()
        });
    }

    public override Task<VerifyHeightIndexResponse> VerifyHeightIndex(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new VerifyHeightIndexResponse
        {
            Err = Error.Unspecified
        });
    }

    public override Task<CreateHandlersResponse> CreateHandlers(Empty request, ServerCallContext context)
    {
        var result = new CreateHandlersResponse();
        result.Handlers.Add(new Handler { ServerAddr = $"localhost:{_globalSettings.VmPort}" });
        return Task.FromResult(result);
    }

    public override Task<StateSyncEnabledResponse> StateSyncEnabled(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new StateSyncEnabledResponse
        {
            Enabled = true,
            Err = Error.Unspecified
        });
    }

    public override Task<GetOngoingSyncStateSummaryResponse> GetOngoingSyncStateSummary(Empty request,
        ServerCallContext context)
    {
        var lastBlock = _blockchain.LatestBlock;
        return Task.FromResult(new GetOngoingSyncStateSummaryResponse
        {
            Bytes = lastBlock.SerializeJsonByteString(),
            Err = Error.Unspecified,
            Height = lastBlock.Height,
            Id = lastBlock.Hash.HexStringToByteString()
        });
    }

    public override Task<SetStateResponse> SetState(SetStateRequest request, ServerCallContext context)
    {
        _blockchain.State = request.State;
        var lastBlock = _blockchain.LatestBlock;
        return Task.FromResult(new SetStateResponse
        {
            Bytes = lastBlock.SerializeJsonByteString(),
            Height = lastBlock.Height,
            LastAcceptedId = lastBlock.Hash.HexStringToByteString(),
            LastAcceptedParentId = lastBlock.ParentHash.HexStringToByteString(),
            Timestamp = lastBlock.Timestamp.LongToTimestamp()
        });
    }

    public override Task<Empty> Connected(ConnectedRequest request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }

    public override Task<HealthResponse> Health(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new HealthResponse());
    }

    public override Task<ParseBlockResponse> ParseBlock(ParseBlockRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Parse block {body}", request.Bytes.ToStringUtf8());
        var block = request.Bytes.DeserializeJsonByteString<Block>();
        if (block == null) return Task.FromResult(new ParseBlockResponse());
        _blockchain.AddPendingBlock(block);
        return Task.FromResult(new ParseBlockResponse
        {
            Height = block.Height,
            Id = block.Hash.HexStringToByteString(),
            ParentId = block.ParentHash.HexStringToByteString(),
            Status = Status.Processing,
            VerifyWithContext = false,
            Timestamp = block.Timestamp.LongToTimestamp()
        });
    }

    public override Task<Empty> AppGossip(AppGossipMsg request, ServerCallContext context)
    {
        var transaction = request.Msg.DeserializeJsonByteString<Transaction>();
        if (transaction != null) _blockchain.AddPendingTransaction(transaction);
        return Task.FromResult(new Empty());
    }

    public override Task<BuildBlockResponse> BuildBlock(BuildBlockRequest request, ServerCallContext context)
    {
        var block = _blockchain.BuildBlock(_blockchain.TransactionPool.Values);
        _logger.LogInformation("Built block {block}", block);
        return Task.FromResult(new BuildBlockResponse
        {
            Bytes = block.SerializeJsonByteString(),
            Height = block.Height,
            Id = block.Hash.HexStringToByteString(),
            ParentId = block.ParentHash.HexStringToByteString(),
            Timestamp = block.Timestamp.LongToTimestamp(),
            VerifyWithContext = false
        });
    }

    public override Task<BlockVerifyResponse> BlockVerify(BlockVerifyRequest request, ServerCallContext context)
    {
        var block = request.Bytes.DeserializeJsonByteString<Block>();
        if (_blockchain.Verify(block))
            return Task.FromResult(new BlockVerifyResponse
            {
                Timestamp = block.Timestamp.LongToTimestamp()
            });
        return Task.FromResult(new BlockVerifyResponse());
    }

    public override Task<Empty> SetPreference(SetPreferenceRequest request, ServerCallContext context)
    {
        _blockchain.SetPreference((Id)request.Id);
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> BlockAccept(BlockAcceptRequest request, ServerCallContext context)
    {
        _blockchain.Accept((Id)request.Id);
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> BlockReject(BlockRejectRequest request, ServerCallContext context)
    {
        _blockchain.Reject((Id)request.Id);
        return Task.FromResult(new Empty());
    }

    public override Task<GetBlockResponse> GetBlock(GetBlockRequest request, ServerCallContext context)
    {
        var (block, status) = _blockchain.GetBlock((Id)request.Id);
        if (block == null)
            return Task.FromResult(new GetBlockResponse
            {
                Err = Error.NotFound
            });
        return Task.FromResult(new GetBlockResponse
        {
            Bytes = block.SerializeJsonByteString(),
            Err = Error.Unspecified,
            Height = block.Height,
            ParentId = block.ParentHash.HexStringToByteString(),
            Status = status,
            Timestamp = block.Timestamp.LongToTimestamp(),
            VerifyWithContext = false
        });
    }
}