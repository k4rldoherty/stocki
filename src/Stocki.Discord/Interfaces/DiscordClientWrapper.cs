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

    public async ValueTask<IDiscordUser?> GetUserAsync(ulong userId)
    {
        var user = await _client.GetUserAsync(userId);
        return user != null ? new DiscordUserWrapper((SocketUser)user) : null;
    }
}
