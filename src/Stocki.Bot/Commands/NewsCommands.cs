using Discord;
using Discord.Interactions;
using MediatR;
using Microsoft.Extensions.Logging;
using Stocki.Application.Exceptions;
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
            if (articles != null)
            {
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
        }
        catch (StockDataNotFoundException ex)
        {
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Information Not Found") // Clear, user-centric title
                    .AddField("Message", ex.UserFriendlyMessage) // Directly use the user-friendly message from the exception
                    .WithColor(Color.Orange) // A warning/informational color
                    .WithFooter("Stocki 2025")
                    .Build()
            );
        }
        catch (ExternalServiceException ex)
        {
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Exteral Service Error")
                    .AddField("Message", ex.UserFriendlyMessage)
                    .WithColor(Color.Red) // Critical error color
                    .WithFooter("If this persists, please contact support.")
                    .Build()
            );
        }
        catch (ArgumentException ex) // Catch validation errors from TickerSymbol or other ArgumentExceptions
        {
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("Input Error")
                    .WithDescription($"The ticker '{ticker}' is invalid. Reason: {ex.Message}")
                    .WithColor(Color.Orange) // Use a different color for input errors
                    .Build()
            );
        }
        catch (Exception ex) // Catch any other unexpected errors
        {
            _logger.LogError(ex.Message);
            await FollowupAsync(
                embed: new EmbedBuilder()
                    .WithTitle("System Error")
                    .WithDescription(
                        "An unexpected error occurred while processing your request. Please try again later."
                    )
                    .WithFooter("If this persists, contact support.")
                    .WithColor(Color.Red)
                    .Build()
            );
        }
    }
}
