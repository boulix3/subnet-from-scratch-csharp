using Grpc.Net.Client;
using Messenger;
using MySubnet.Shared;

namespace MySubnet.Avalanche;

public class MessengerClient
{
    private readonly Messenger.Messenger.MessengerClient _grpcClient;
    private readonly ILogger _logger;
    private readonly GlobalSettings _settings;

    public MessengerClient(GlobalSettings settings, ILogger<MessengerClient> logger)
    {
        _logger = logger;
        _settings = settings;
        _grpcClient =
            new Messenger.Messenger.MessengerClient(GrpcChannel.ForAddress($"http://{settings.AvalancheRpcUrl}"));
    }

    public async Task NotifyBuildBlock()
    {
        try
        {
            await _grpcClient.NotifyAsync(new NotifyRequest
            {
                Message = Message.BuildBlock
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while sending notify to avalanche ({url})", _settings.AvalancheRpcUrl);
        }
    }
}