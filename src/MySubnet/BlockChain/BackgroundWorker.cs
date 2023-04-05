using MySubnet.Avalanche;

namespace MySubnet.BlockChain;

internal class BackgroundWorkerService : BackgroundService
{
    private readonly BlockChain _blockchain;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public BackgroundWorkerService(ILogger<BackgroundWorkerService> logger, BlockChain blockchain,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _blockchain = blockchain;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var seconds = 1;
        _logger.LogInformation("Background worker started with {seconds}s interval", seconds);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(seconds));
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
            await Tick();
        _logger.LogInformation("Background worker stopped");
    }

    private Task Tick()
    {
        if (_blockchain.ShouldBuildBlock())
            return _serviceProvider.GetRequiredService<MessengerClient>().NotifyBuildBlock();
        return Task.CompletedTask;
    }
}