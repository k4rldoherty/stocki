namespace Stocki.Application.Interfaces;

public interface IGeminiClient
{
    public ValueTask<string> GetResponseAsync(string prompt, CancellationToken c);
}
