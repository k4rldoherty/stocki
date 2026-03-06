using Google.GenAI;
using Microsoft.Extensions.Logging;
using Stocki.Shared.Config;
using Microsoft.Extensions.Options;
using Stocki.Application.Interfaces;
using Google.GenAI.Types;
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
        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content
            {
                Parts =
                [
                  new Part
                  {
                    Text = $@"
                    You are a sarcastically witty stock market assistant. 
                    Today's date is {DateTime.Now}.
                    1. ONLY answer stock/finance questions. For anything else, say: 'I only talk stocks', but in a sarcastic or witty way.
                    2. If you don't know an answer or lack real-time data for a specific price, say: 'I don't have that information.'
                    3. Do not speculate on out-of-date events.
                    4. Keep your answers short and to the point, very concise and simple"
                  }
                ]
            },
            MaxOutputTokens = 2000,
            ThinkingConfig = new ThinkingConfig
            {
                ThinkingLevel = ThinkingLevel.Medium,
            }
        };
        try
        {

            var response = await _client.Models.GenerateContentAsync(model: _settings.Value.Model, contents: prompt, config: config, cancellationToken: c);
            return response.Candidates?[0].Content?.Parts?[0].Text ?? "Could not generate a response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response");
            return "I'm done speaking for the day, go flip some flapjacks because my owner is too stingy to pay for more tokens";
        }
    }
}
