using Discord.WebSocket;
using Stocki.Application.Interfaces;
using Stocki.Discord.Interfaces;

namespace Stocki.Discord.Handlers;

public class MessageHandler
{
    private readonly IGeminiClient _client;
    private readonly ILogger<MessageHandler> _logger;
    private readonly IDiscordClientWrapper _discordClient;

    public MessageHandler(IGeminiClient client, ILogger<MessageHandler> logger, IDiscordClientWrapper discordClient)
    {
        _client = client;
        _logger = logger;
        _discordClient = discordClient;
    }

    public async Task HandleMessageAsync(IDiscordMessage msg)
    {
        if (msg.Author.IsBot)
        {
            return;
        }

        var mentions = msg.MentionedUsers;
        if (!mentions.Any(u => u.Id == _discordClient.CurrentUserId))
            return;

        var prompt = msg.Content;
        foreach (var user in mentions)
            prompt = prompt.Replace(user.Mention, "").Trim();

        if (string.IsNullOrWhiteSpace(prompt))
            await msg.Channel.SendMessageAsync("Please enter a prompt.");

        _ = Task.Run(async () =>
            {
                try
                {

                    using (var channel = msg.Channel.EnterTypingState())
                    {

                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
                        var response = await _client.GetResponseAsync(prompt, cts.Token);
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

        await Task.CompletedTask;
    }
}
