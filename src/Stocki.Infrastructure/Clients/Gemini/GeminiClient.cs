using Google.GenAI;
using Microsoft.Extensions.Logging;
using Stocki.Shared.Config;
using Microsoft.Extensions.Options;
using Stocki.Application.Interfaces;
namespace Stocki.Infrastructure.Clients;

public class GeminiClient : IGeminiClient
{
    private readonly ILogger<GeminiClient> _logger;
    private readonly IOptions<GeminiSettings> _settings;
    private readonly Client _client;
    public GeminiClient(ILogger<GeminiClient> logger, IOptions<GeminiSettings> settings, Client client)
    {
        _logger = logger;
        _settings = settings;
        _client = client;
    }

    public async ValueTask<string> GetResponseAsync(string prompt, CancellationToken c)
    {
        var response = await _client.Models.GenerateContentAsync(model: _settings.Value.Model, contents: prompt, config: null, c);
        return response.Candidates?[0].Content?.Parts?[0].Text ?? "Could not generate a response.";
    }
}
