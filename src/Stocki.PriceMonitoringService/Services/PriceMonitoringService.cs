using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Stocki.PriceMonitor.Services;

public sealed class PriceMonitoringService : BackgroundService
{
    private readonly ILogger<PriceMonitoringService> _logger;
    private readonly FinnhubWSManager _wsManager;

    public PriceMonitoringService(ILogger<PriceMonitoringService> logger, FinnhubWSManager wsManger)
    {
        _logger = logger;
        _wsManager = wsManger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Price monitor service starting ....");
            try
            {
                await _wsManager.ConnectAndListenAsync(stoppingToken);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex.Message);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken token)
    {
        // Call the finnhub stop websockets connection logic here
        await _wsManager.StopAsync(token);
        await base.StopAsync(token);
    }
}
