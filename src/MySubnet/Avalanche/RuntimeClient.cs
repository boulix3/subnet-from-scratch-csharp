using System.Globalization;
using System.Text.RegularExpressions;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting.Server.Features;
using MySubnet.Shared;
namespace MySubnet.Avalanche;

public class RuntimeClient
{
    private readonly Vm.Runtime.Runtime.RuntimeClient _grpcClient;
    private readonly uint _protocolVersion;
    private readonly ILogger _logger;
    public RuntimeClient(GlobalSettings settings, ILogger<RuntimeClient> logger)
    {
        this._logger = logger;
        _grpcClient = new(GrpcChannel.ForAddress($"http://{settings.Url}"));
        _protocolVersion = settings.ProtocolVersion;
    }

    public void Initialize(WebApplication app)
    {
        var address = GetServerAddress(app);
        _logger.LogInformation("Calling Runtime.Initialize on {@address}", address);
        _grpcClient.Initialize(new Vm.Runtime.InitializeRequest
        {
            Addr = address,
            ProtocolVersion = _protocolVersion
        });
    }

    private static string GetServerAddress(WebApplication app)
    {
        var url = app.Urls.FirstOrDefault();
        if (url != null)
        {
            var port = GetPort(url);
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