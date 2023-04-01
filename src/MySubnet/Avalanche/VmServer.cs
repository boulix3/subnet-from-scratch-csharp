using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MySubnet.Shared;
using Vm;

namespace MySubnet.Avalanche;
public class VmServer : Vm.VM.VMBase
{
    private readonly ILogger _logger;
    private readonly GlobalSettings _globalSettings;

    public VmServer(ILogger<VmServer> logger, GlobalSettings globalSettings)
    {
        _logger = logger;
        _globalSettings = globalSettings;
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
        base.Initialize
        return Task.FromResult(new InitializeResponse()
        {
            Bytes = ByteString.Empty,
            Height = 0,
            LastAcceptedId = Id.Default,
            LastAcceptedParentId = Id.Default,
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
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
        return Task.FromResult(new CreateHandlersResponse
        {
        });
    }

    public override Task<StateSyncEnabledResponse> StateSyncEnabled(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new StateSyncEnabledResponse
        {
            Enabled = true,
            Err = Error.Unspecified
        });
    }

    public override Task<GetOngoingSyncStateSummaryResponse> GetOngoingSyncStateSummary(Empty request, ServerCallContext context)
    {
        return Task.FromResult(new GetOngoingSyncStateSummaryResponse
        {
            Bytes = ByteString.Empty,
            Err = Error.Unspecified,
            Height = 0,
            Id = Id.Default
        });
    }
    public override Task<SetStateResponse> SetState(SetStateRequest request, ServerCallContext context)
    {
        return Task.FromResult(new SetStateResponse
        {
            Bytes = ByteString.Empty,
            Height = 0,
            LastAcceptedId = Id.Default,
            LastAcceptedParentId = Id.Default,
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
        });
    }

    public override Task<Empty> SetPreference(SetPreferenceRequest request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
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
        return Task.FromResult(new ParseBlockResponse
        {
            Height = 0,
            Id = Id.Default,
            ParentId = Id.Default,
            Status = Vm.Status.Processing,
            VerifyWithContext = false
        });
    }
}