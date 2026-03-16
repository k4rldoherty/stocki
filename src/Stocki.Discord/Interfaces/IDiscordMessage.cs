using Discord;

namespace Stocki.Discord.Interfaces;

public interface IDiscordUser
{
    bool IsBot { get; }
    ulong Id { get; }
    string Mention { get; }
}

public interface IDiscordMessageChannel
{
    Task SendMessageAsync(string? text = null, bool isTTS = false, Embed? embed = null, RequestOptions? options = null);
    IDisposable EnterTypingState(RequestOptions? options = null);
}

public interface IDiscordMessage
{
    IDiscordUser Author { get; }
    string Content { get; }
    IReadOnlyCollection<IDiscordUser> MentionedUsers { get; }
    IDiscordMessageChannel Channel { get; }
}
