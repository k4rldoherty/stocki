using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stocki.Application.Commands.PriceSubscribe;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Quote;
using Stocki.Domain.ValueObjects;

namespace Stocki.PriceMonitor.Services;

public sealed class PriceMonitoringService
    : BackgroundService,
        INotificationHandler<PriceSubscribeCommand>
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
        PriceSubscribeCommand notification,
        CancellationToken cancellationToken
    )
    {
        using (var s = _serviceScopeFactory.CreateScope())
        {
            var client = s.ServiceProvider.GetRequiredService<IFinnhubClient>();
            var res = await client.GetStockQuoteAsync(
                new StockQuoteQuery(new TickerSymbol(notification.Symbol.Value)),
                cancellationToken
            );
            if (res.Data == null)
                return;
        }
        await _wsManager.SendMessageAsync(cancellationToken, notification.Symbol.Value);
        _priceChecker._stockPrices.TryAdd(notification.Symbol.Value, 0.0);
    }
}
