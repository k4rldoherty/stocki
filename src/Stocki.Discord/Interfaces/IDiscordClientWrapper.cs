using Discord;

namespace Stocki.Discord.Interfaces;

public interface IDiscordClientWrapper
{
    ulong CurrentUserId { get; }
    ValueTask<IUser?> GetUserAsync(ulong userId);
}
