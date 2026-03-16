using Discord;
using Discord.WebSocket;

namespace Stocki.Discord.Interfaces;

public class DiscordUserWrapper : IDiscordUser
{
    private readonly SocketUser _user;

    public DiscordUserWrapper(SocketUser user)
    {
        _user = user;
    }

    public bool IsBot => _user.IsBot;
    public ulong Id => _user.Id;
    public string Mention => _user.Mention;

    public async ValueTask<IDiscordDmChannel> CreateDMChannelAsync()
    {
        var channel = await _user.CreateDMChannelAsync();
        return new DiscordDmChannelWrapper(channel);
    }
}

public class DiscordMessageChannelWrapper : IDiscordMessageChannel
{
    private readonly SocketTextChannel _channel;

    public DiscordMessageChannelWrapper(SocketTextChannel channel)
    {
        _channel = channel;
    }

    public Task SendMessageAsync(string? text = null, bool isTTS = false, Embed? embed = null, RequestOptions? options = null)
    {
        return _channel.SendMessageAsync(text, isTTS, embed, options);
    }

    public IDisposable EnterTypingState(RequestOptions? options = null)
    {
        return _channel.EnterTypingState(options);
    }
}

public class DiscordMessageWrapper : IDiscordMessage
{
    private readonly SocketMessage _message;

    public DiscordMessageWrapper(SocketMessage message)
    {
        _message = message;
    }

    public IDiscordUser Author => new DiscordUserWrapper(_message.Author);
    public string Content => _message.Content;
    public IReadOnlyCollection<IDiscordUser> MentionedUsers => 
        _message.MentionedUsers.Select(u => new DiscordUserWrapper(u)).ToList();
    public IDiscordMessageChannel Channel => new DiscordMessageChannelWrapper((SocketTextChannel)_message.Channel);
}
