namespace Stocki.Discord.Interfaces;

public interface IDiscordClientWrapper
{
    ulong CurrentUserId { get; }
    ValueTask<IDiscordUser?> GetUserAsync(ulong userId);
}
