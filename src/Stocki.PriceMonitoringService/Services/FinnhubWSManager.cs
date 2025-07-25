using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
    private readonly ClientWebSocket _webSocketClient;
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
        _webSocketClient = new ClientWebSocket();
        _logger = logger;
        _scopeFactory = scopeFactory;
        _priceChecker = priceChecker;
    }

    public async Task ConnectAndListenAsync(CancellationToken token)
    {
        try
        {
            await _webSocketClient.ConnectAsync(_uri, token);
            _recieveCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            _sendCts = CancellationTokenSource.CreateLinkedTokenSource(token);
            using (var scopeFactory = _scopeFactory.CreateScope())
            {
                var repo =
                    scopeFactory.ServiceProvider.GetRequiredService<IStockPriceSubscriptionRepository>();
                var fhClient = scopeFactory.ServiceProvider.GetRequiredService<IFinnhubClient>();
                var subs = await repo.GetAllSubscriptionsAsync(token);
                foreach (var s in subs)
                {
                    await SendMessageAsync(_sendCts.Token, s.Ticker, true);
                    // Get the intial price of all subscribed stocks
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

            _logger.LogInformation("[WS] Client Listening for messages.");
            await RecieveMessagesAsync(_recieveCts.Token);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation("[WS] Operation Cancelled: {ex}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("[WS] Unhandled Exception: {ex}", ex.Message);
        }
        finally
        {
            _recieveCts?.Cancel();
            _sendCts?.Cancel();
            await _webSocketClient.CloseAsync(WebSocketCloseStatus.Empty, "", token);
            _webSocketClient.Dispose();
        }
    }

    public async Task StopAsync(CancellationToken token)
    {
        _recieveCts?.Cancel();
        _sendCts?.Cancel();

        if (
            _webSocketClient?.State == WebSocketState.Open
            || _webSocketClient?.State == WebSocketState.Connecting
        )
        {
            Console.WriteLine("[WS] Explicitly closing WebSocket on StopAsync...");
            await _webSocketClient.CloseOutputAsync(
                WebSocketCloseStatus.NormalClosure,
                "StopAsync called",
                CancellationToken.None
            );
            await _webSocketClient.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "StopAsync called",
                CancellationToken.None
            );
        }
        _webSocketClient?.Dispose();
        _logger.LogInformation("[WS] WebSocket client stopped and disposed.");
    }

    private async Task RecieveMessagesAsync(CancellationToken token)
    {
        var buffer = new byte[1024 * 4];
        try
        {
            while (_webSocketClient.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                var result = await _webSocketClient.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    token
                );

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    // skip all non trade message types
                    // bit rough but will do for now
                    if (receivedMessage.Count() < 25)
                    {
                        continue;
                    }
                    FinnhubStockPriceRecievedMessage ParsedWebsocketMessage;
                    try
                    {
                        ParsedWebsocketMessage =
                            JsonConvert.DeserializeObject<FinnhubStockPriceRecievedMessage>(
                                receivedMessage
                            );
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(
                            "[WS] Recieved message but did not parse successfully: {}",
                            ex.Message
                        );
                        continue;
                    }
                    if (ParsedWebsocketMessage.Data.Count() == 0)
                    {
                        _logger.LogWarning("Invalid message recieved");
                        continue;
                    }
                    _priceChecker.CheckPrice(ParsedWebsocketMessage);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation(
                        $"[WS] Server initiated close: {result.CloseStatus} - {result.CloseStatusDescription}"
                    );
                    // Attempt to close from client side if server initiates
                    await _webSocketClient.CloseOutputAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Client ack close",
                        CancellationToken.None
                    );
                    break; // Exit loop
                }
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation("[WS] Recieving canceled: {ex}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("[WS] Unexpected exception {ex}", ex.Message);
        }
    }

    public async Task SendMessageAsync(CancellationToken token, string symbol, bool isSubscribe)
    {
        if (isSubscribe)
        {
            try
            {
                _logger.LogInformation("Subscribing to {}", symbol);
                var message = new FinnhubWebsocketSubscriptionMessage
                {
                    Type = "subscribe",
                    Symbol = symbol,
                };
                byte[] bytesToSend = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                await _webSocketClient.SendAsync(
                    new ArraySegment<byte>(bytesToSend),
                    WebSocketMessageType.Text,
                    true,
                    token
                );
                _logger.LogInformation($"[WS] Sent: '{message.Symbol}'");
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
        else
        {
            try
            {
                _logger.LogInformation("Unsubscribing from {}", symbol);
                var message = new FinnhubWebsocketSubscriptionMessage
                {
                    Type = "unsubscribe",
                    Symbol = symbol,
                };
                byte[] bytesToSend = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                await _webSocketClient.SendAsync(
                    new ArraySegment<byte>(bytesToSend),
                    WebSocketMessageType.Text,
                    true,
                    token
                );
                _logger.LogInformation($"[WS] Sent: '{message.Symbol}'");
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
}
