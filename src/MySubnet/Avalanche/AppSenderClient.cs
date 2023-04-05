using Appsender;
using Grpc.Net.Client;
using MySubnet.BlockChain;
using MySubnet.Shared;

namespace MySubnet.Avalanche;

public class AppSenderClient
{
    private readonly AppSender.AppSenderClient _grpcClient;
    private readonly ILogger _logger;
    private readonly GlobalSettings _settings;

    public AppSenderClient(GlobalSettings settings, ILogger<AppSenderClient> logger)
    {
        _logger = logger;
        _settings = settings;
        _grpcClient = new AppSender.AppSenderClient(GrpcChannel.ForAddress($"http://{settings.AvalancheRpcUrl}"));
    }

    public async Task GossipTransaction(Transaction transaction)
    {
        try
        {
            await _grpcClient.SendAppGossipAsync(new SendAppGossipMsg
            {
                Msg = transaction.SerializeJsonByteString()
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while sending gossip to avalanche ({url})", _settings.AvalancheRpcUrl);
        }
    }
}