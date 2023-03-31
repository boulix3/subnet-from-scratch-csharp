using System.Reflection;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Vm;

namespace MySubnet.Avalanche;
public class VmServer : Vm.VM.VMBase
{
    private readonly ILogger _logger;
    public VmServer(ILogger<VmServer> logger)
    {
        _logger = logger;
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
}