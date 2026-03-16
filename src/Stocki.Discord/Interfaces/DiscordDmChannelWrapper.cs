using Discord;

namespace Stocki.Discord.Interfaces;

public class DiscordDmChannelWrapper : IDiscordDmChannel
{
    private readonly IDMChannel _channel;

    public DiscordDmChannelWrapper(IDMChannel channel)
    {
        _channel = channel;
    }

    public Task SendMessageAsync(string? text = null, bool isTTS = false, Embed? embed = null, RequestOptions? options = null)
    {
        return _channel.SendMessageAsync(text, isTTS, embed, options);
    }
}
