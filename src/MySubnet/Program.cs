using Microsoft.AspNetCore.Server.Kestrel.Core;
using MySubnet.Avalanche;
using MySubnet.BlockChain;
using MySubnet.Shared;

if (args.Length == 1 && args[0] == "genesis")
{
    Genesis.Create();
    return;
}

const int ProtocolVersion = 24;

// Get the Avalanche runtime grpc address from the environment variable. Fail fast if not defined.
var avalancheVmRuntimeEngineAddr
    = Environment.GetEnvironmentVariable("AVALANCHE_VM_RUNTIME_ENGINE_ADDR")
      ?? throw new ArgumentException("AVALANCHE_VM_RUNTIME_ENGINE_ADDR environment variable is null");
var globalSettings = new GlobalSettings(avalancheVmRuntimeEngineAddr, new CancellationTokenSource(), ProtocolVersion);

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    serverOptions.ListenAnyIP(0, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
});

builder.Services.AddSingleton(globalSettings);
builder.Services.AddHostedService<BackgroundWorkerService>();
builder.Services.AddScoped<RuntimeClient>();
builder.Services.AddScoped<AppSenderClient>();
builder.Services.AddScoped<MessengerClient>();
builder.Services.AddSingleton<BlockChain>();

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.MapGrpcService<VmServer>();
app.MapGrpcService<HttpServer>();

await app.StartAsync(globalSettings.TokenSource.Token);
var runtimeClient = app.Services.GetRequiredService<RuntimeClient>();
runtimeClient.Initialize(app);
while (!globalSettings.TokenSource.IsCancellationRequested) await Task.Delay(1000); // Keep awake
app.Logger.LogInformation("Application ended");