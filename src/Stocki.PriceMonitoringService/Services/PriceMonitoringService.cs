using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stocki.Shared.Notifications;

namespace Stocki.PriceMonitor.Services;

public class PriceMonitoringService
    : BackgroundService,
        INotificationHandler<PriceSubscribedNotification>,
        INotificationHandler<PriceUnsubscribedNotification>
{
    private readonly ILogger<PriceMonitoringService> _logger;
    private readonly FinnhubWSManager _wsManager;
    private readonly PriceChecker _priceChecker;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PriceMonitoringService(
        ILogger<PriceMonitoringService> logger,
        FinnhubWSManager wsManger,
        PriceChecker priceChecker,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        _wsManager = wsManger;
        _priceChecker = priceChecker;
        _serviceScopeFactory = serviceScopeFactory;
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
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken token)
    {
        // Call the finnhub stop websockets connection logic here
        await _wsManager.StopAsync(token);
        await base.StopAsync(token);
    }

    public async Task Handle(
        PriceSubscribedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _wsManager.SendMessageAsync(cancellationToken, notification.Symbol, true);
        _priceChecker._stockPrices.TryAdd(notification.Symbol, 0.0);
    }

    public async Task Handle(
        PriceUnsubscribedNotification notification,
        CancellationToken cancellationToken
    )
    {
        await _wsManager.SendMessageAsync(cancellationToken, notification.Symbol, false);
        _priceChecker._stockPrices.Remove(notification.Symbol, out var _);
    }
}
