using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LawEnforcementDialer.PinManager;

public sealed class PinMonitor : BackgroundService
{
    private readonly IPinManager _pinManager;
    private readonly IOptionsMonitor<PinManagerConfiguration> _configuration;

    public PinMonitor(IPinManager pinManager, IOptionsMonitor<PinManagerConfiguration> configuration)
    {
        _pinManager = pinManager;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _pinManager.ResetBadAttempts(DateTimeOffset.UtcNow);
            await Task.Delay(TimeSpan.FromMinutes(_configuration.CurrentValue.PinMonitorSleepInMinutes), stoppingToken);
        }
    }
}