using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stocki.Application.Interfaces;
using Stocki.Application.Queries.Quote;
using Stocki.Domain.Interfaces;
using Stocki.Domain.ValueObjects;
using Stocki.PriceMonitor.Models;
using Stocki.Shared.Config;

namespace Stocki.PriceMonitor.Services;

public class FinnhubWSManager
{
    private readonly Uri _uri;
    private ClientWebSocket? _webSocketClient;
    private CancellationTokenSource? _recieveCts;
    private CancellationTokenSource? _sendCts;
    private ILogger<FinnhubWSManager> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private IOptions<FinnhubWebsocketsSettings> _options;
    private PriceChecker _priceChecker;

    public FinnhubWSManager(
        ILogger<FinnhubWSManager> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<FinnhubWebsocketsSettings> options,
        PriceChecker priceChecker
        )
    {
        _options = options;
        _uri = new Uri($"{_options.Value.BaseUrl}?token={_options.Value.ApiKey}");
        _logger = logger;
        _scopeFactory = scopeFactory;
        _priceChecker = priceChecker;
    }

    public async Task ConnectAndListenAsync(CancellationToken token)
    {
        using var client = new ClientWebSocket();
        _webSocketClient = client;
        try
        {
            await client.ConnectAsync(_uri, token);
            _recieveCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _sendCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            await GetSubscribedStocksAndInitialPricesAsync(token);
            _logger.LogInformation("[WS] Client Listening for messages.");
            await RecieveMessagesAsync(_recieveCts.Token, client);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation("[WS] Operation Cancelled: {ex}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("[WS] Unhandled Exception: {ex}", ex.Message);
            throw;
        }
        finally
        {
            if (_webSocketClient == client) _webSocketClient = null;
        }
    }

    private async Task GetSubscribedStocksAndInitialPricesAsync(CancellationToken token)
    {
        using (var scopeFactory = _scopeFactory.CreateScope())
        {
            var repo =
              scopeFactory.ServiceProvider.GetRequiredService<IStockPriceSubscriptionRepository>();
            var fhClient = scopeFactory.ServiceProvider.GetRequiredService<IFinnhubClient>();
            var subs = await repo.GetAllSubscriptionsAsync(token);
            foreach (var s in subs)
            {
                await SendMessageAsync(_sendCts!.Token, s.Ticker, true);
                var initialQuote = await fhClient.GetStockQuoteAsync(
                    new StockQuoteQuery(new TickerSymbol(s.Ticker)),
                    token
                    );
                if (initialQuote.Data != null)
                {
                    _priceChecker._stockPrices.TryAdd(
                        initialQuote.Data.Ticker,
                        initialQuote.Data.CurrentPrice
                        );
                }
            }
        }
    }

    public async Task StopAsync(CancellationToken token)
    {
        var client = _webSocketClient;
        _recieveCts?.Cancel();
        _sendCts?.Cancel();

        if (
            client?.State == WebSocketState.Open
            || client?.State == WebSocketState.Connecting
           )
        {
            Console.WriteLine("[WS] Explicitly closing WebSocket on StopAsync...");
            await client.CloseOutputAsync(
                WebSocketCloseStatus.NormalClosure,
                "StopAsync called",
                CancellationToken.None
                );
            await client.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "StopAsync called",
                CancellationToken.None
                );
        }
        client?.Dispose();
        _logger.LogInformation("[WS] WebSocket client stopped and disposed.");
    }

    private async Task RecieveMessagesAsync(CancellationToken token, ClientWebSocket client)
    {
        if (client == null)
        {
            _logger.LogWarning("[WS] Cannot revieve message: No active connection.");
            return;
        }
        var buffer = new byte[1024 * 4];
        using var ms = new MemoryStream();
        try
        {
            while (client.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                ms.SetLength(0);
                do
                {
                    result = await client.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        token);

                    if (result.MessageType == WebSocketMessageType.Close) break;

                    ms.Write(buffer.AsSpan(0, result.Count));

                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close) break;

                if (ms.Length > 0)
                {
                    if (ms.Length < 25) continue;
                    try
                    {
                        ms.Seek(0, SeekOrigin.Begin);
                        var parsedWebsocketMessage = await JsonSerializer.DeserializeAsync<FinnhubStockPriceReceivedMessage>(ms, cancellationToken: token);
                        if (parsedWebsocketMessage.Data == null || parsedWebsocketMessage.Data.Length == 0)
                        {
                            _logger.LogWarning("Invalid message recieved");
                            continue;
                        }
                        _priceChecker.CheckPrice(parsedWebsocketMessage, (decimal)_options.Value.PriceChangePercentage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            "[WS] Recieved message but did not parse successfully: {}",
                            ex.Message
                            );
                    }
                }
                else
                {
                    _logger.LogInformation("[WS] MemoryStream is empty.");
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "[WS] Fatal loop error");
        }
    }


    public async Task SendMessageAsync(CancellationToken token, string symbol, bool isSubscribe)
    {
        var client = _webSocketClient;
        if (client == null)
        {
            return;
        }
        if (isSubscribe)
        {
            await SendNewSubscriptionAsync(symbol, token, client);
        }
        else
        {
            await SendNewUnsubscriptionAsync(symbol, token, client);
        }
    }

    private async Task SendNewSubscriptionAsync(string symbol, CancellationToken token, ClientWebSocket client)
    {
        try
        {
            var message = new FinnhubWebsocketSubscriptionMessage
            {
                Type = "subscribe",
                Symbol = symbol,
            };
            byte[] bytesToSend = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await client.SendAsync(
                new ArraySegment<byte>(bytesToSend),
                WebSocketMessageType.Text,
                true,
                token
                );
            _logger.LogInformation($"[WS]Subscribe Sent: '{message.Symbol}'");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[WS] Sending cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"[WS] Error sending: {ex.Message}");
        }
    }

    private async Task SendNewUnsubscriptionAsync(string symbol, CancellationToken token, ClientWebSocket client)
    {
        try
        {
            var message = new FinnhubWebsocketSubscriptionMessage
            {
                Type = "unsubscribe",
                Symbol = symbol,
            };
            byte[] bytesToSend = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            await client.SendAsync(
                new ArraySegment<byte>(bytesToSend),
                WebSocketMessageType.Text,
                true,
                token
                );
            _logger.LogInformation($"[WS]Unsubscribe Sent: '{message.Symbol}'");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[WS] Sending cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"[WS] Error sending: {ex.Message}");
        }
    }
}
