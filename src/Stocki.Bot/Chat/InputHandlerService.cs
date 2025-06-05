using Discord.WebSocket;

namespace Stocki.Bot.Chat;

public class InputHandlerService()
{
    public async Task HandleMessageAsync(SocketMessage msg)
    {
        // Stops the bot from replying to itself
        if (msg.Author.IsBot)
            return;
        // TODO: some text functionality can be implemented here .. AI wrapper or something.

        // Reply to all dm messages or any time it is mentioned.
        await msg.Channel.SendMessageAsync(
            $"Hello {msg.Author.Username}!\nI am currently a work in progress and don't have the brain power to converse with you.\nCheck back soon..."
        );
        return;
    }
}
