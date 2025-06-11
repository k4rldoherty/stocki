namespace Stocki.Shared.Config;

// Use IOptions to make a strongly typed class for options
public class DiscordSettings
{
    public string Token { get; set; } = string.Empty;
}
