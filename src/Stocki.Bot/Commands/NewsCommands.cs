using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Queries.News;
using Stocki.Domain.ValueObjects;

namespace Stocki.Bot.Commands;

public class NewsCommands : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ILogger<NewsCommands> _logger;
    private readonly IMediator _mediator;

    public NewsCommands(ILogger<NewsCommands> logger, IMediator m)
    {
        _logger = logger;
        _mediator = m;
    }

    [SlashCommand("get-company-news", "gets recent news articles for a given stock eg .AAPl")]
    public async Task HandleGetCompanyNewsAsync(
        [Summary("ticker", "the ticker of the stock you are querying eg AAPL")] string ticker
    )
    {
        await DeferAsync();
        try
        {
            var symbol = new TickerSymbol(ticker);
            var q = new StockNewsQuery(symbol);
            _logger.LogInformation(
                "Received /get-company-news command for ticker {Ticker}",
                ticker
            );
            var articles = await _mediator.Send(q, CancellationToken.None);
            var embedBuilder = new EmbedBuilder();
            if (articles == null)
            {
                _logger.LogWarning(
                    "The /news request failed for ticker {Ticker}. No data found.",
                    ticker
                );
                await FollowupAsync(
                    embed: embedBuilder
                        .WithTitle("Error")
                        .WithDescription(
                            $"Could not retrieve news for **{ticker.ToUpperInvariant()}**."
                        )
                        .AddField(
                            "Reason",
                            "The ticker symbol might be incorrect, or data is temporarily unavailable. Please check the ticker and try again."
                        )
                        .WithFooter("Stocki 2025")
                        .WithColor(Color.Red)
                        .Build()
                );
                return;
            }
            embedBuilder.WithTitle($"News articles for {ticker.ToUpper()}");
            embedBuilder.WithFooter($"Data provided by Finnhub");
            foreach (var a in articles)
            {
                string fieldName = $"**{a.Headline}**";
                string fieldValue = $"\n\n{a.Summary}\n\n";
                fieldValue += $"Source: [{a.Source}]({a.Url}) | Date: {a.DateOfArticle}\n-----";
                embedBuilder.AddField(fieldName, fieldValue, false);
            }

            await FollowupAsync(embed: embedBuilder.Build());
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return;
        }
    }
}
