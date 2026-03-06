using Discord.WebSocket;
using Stocki.Application.Interfaces;

namespace Stocki.Discord.Chat;

public class MessageHandler
{
    private readonly IGeminiClient _client;
    private readonly ILogger<MessageHandler> _logger;
    public MessageHandler(IGeminiClient client, ILogger<MessageHandler> logger)
    {
        _client = client;
        _logger = logger;
    }
    public Task HandleMessageAsync(SocketMessage msg)
    {
        // Stops the bot from replying to itself
        if (msg.Author.IsBot)
            return Task.CompletedTask;

        _ = Task.Run(async () =>
            {
                try
                {

                    using (var channel = msg.Channel.EnterTypingState())
                    {

                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
                        var response = await _client.GetResponseAsync(msg.Content, cts.Token);
                        if (response.Length > 2000)
                            await msg.Channel.SendMessageAsync(response[..1997] + "...");
                        else await msg.Channel.SendMessageAsync(response);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling message");
                }
            });
        return Task.CompletedTask;
    }
}
