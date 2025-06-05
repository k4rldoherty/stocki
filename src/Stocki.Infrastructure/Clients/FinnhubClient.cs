namespace Stocki.Infrastructure.Clients;

public class FinnhubClient
{
    private readonly string? _finnhubApiKey =
        Environment.GetEnvironmentVariable("FHAPI")
        ?? throw new NullReferenceException("Cannot retrieve finnhub api key");
}
