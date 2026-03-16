using Discord;
using Discord.WebSocket;

namespace Stocki.Discord.Interfaces;

public class DiscordClientWrapper : IDiscordClientWrapper
{
    private readonly DiscordSocketClient _client;

    public DiscordClientWrapper(DiscordSocketClient client)
    {
        _client = client;
    }

    public ulong CurrentUserId => _client.CurrentUser.Id;

    public ValueTask<IUser?> GetUserAsync(ulong userId)
    {
        return _client.GetUserAsync(userId);
    }
}
