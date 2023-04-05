using System.Globalization;
using System.Text.RegularExpressions;
using Grpc.Net.Client;
using MySubnet.Shared;
using Vm.Runtime;

namespace MySubnet.Avalanche;

public class RuntimeClient
{
    private readonly Runtime.RuntimeClient _grpcClient;
    private readonly ILogger _logger;
    private readonly GlobalSettings _settings;

    public RuntimeClient(GlobalSettings settings, ILogger<RuntimeClient> logger)
    {
        _logger = logger;
        _settings = settings;
        _grpcClient = new Runtime.RuntimeClient(GrpcChannel.ForAddress($"http://{settings.RuntimeUrl}"));
    }

    public void Initialize(WebApplication app)
    {
        var address = GetServerAddress(app);
        _logger.LogInformation("Calling Runtime.Initialize on {@address}", address);
        _grpcClient.Initialize(new InitializeRequest
        {
            Addr = address,
            ProtocolVersion = _settings.ProtocolVersion
        });
    }

    private string GetServerAddress(WebApplication app)
    {
        var url = app.Urls.FirstOrDefault();
        if (url != null)
        {
            var port = GetPort(url);
            _settings.VmPort = port;
            return $"localhost:{port}";
        }

        throw new Exception("Unable to get the WebServer's address");
    }

    private static int GetPort(string serverAddress)
    {
        var regex = "[0-9]+$";
        var match = Regex.Match(serverAddress, regex, RegexOptions.NonBacktracking);
        if (match.Success) return int.Parse(match.Value, CultureInfo.InvariantCulture);
        throw new Exception("Unable to extract port from url " + serverAddress);
    }
}