using Discord.WebSocket;
using Stocki.Application.Interfaces;

namespace Stocki.Discord.Chat;

public class MessageHandler
{
    private readonly IGeminiClient _client;
    public MessageHandler(IGeminiClient client)
    {
        _client = client;
    }
    public Task HandleMessageAsync(SocketMessage msg)
    {
        // Stops the bot from replying to itself
        if (msg.Author.IsBot)
            return Task.CompletedTask;

        _ = Task.Run(async () =>
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                var response = await _client.GetResponseAsync(msg.Content, cts.Token);
                await msg.Channel.SendMessageAsync(response);
            });
        return Task.CompletedTask;
    }
}
