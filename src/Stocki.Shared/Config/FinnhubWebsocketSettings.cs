namespace Stocki.Shared.Config;

// Use IOptions to make a strongly typed class for options
public class FinnhubWebsocketsSettings
{
    public string ApiKey { get; set; } = String.Empty;
    public string BaseUrl { get; set; } = "wss://ws.finnhub.io";
}
