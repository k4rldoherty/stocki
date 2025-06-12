namespace Stocki.Shared.Config;

// Use IOptions to make a strongly typed class for options
public class FinnhubClientSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://finnhub.io/api/v1/";
}
