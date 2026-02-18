namespace Stocki.Domain.Models;

public record StockNewsArticle
{
    public DateTime DateOfArticle;
    public String Headline = string.Empty;
    public String? ImageUrl;
    public String Source = string.Empty;
    public String Summary = string.Empty;
    public String? Url = string.Empty;

    public StockNewsArticle() { }

    public StockNewsArticle(
        long timeStamp,
        String headline,
        String imageUrl,
        String source,
        String summary,
        String? url
    )
    {
        DateOfArticle = ConvertTimestampToDatetime(timeStamp);
        Headline = headline;
        ImageUrl = imageUrl;
        Source = source;
        Summary = summary.Length > 200 ? summary.Substring(0, 100) + "..." : summary;
        Url = url;
    }

    private DateTime ConvertTimestampToDatetime(long ts)
    {
        return DateTime.UnixEpoch.AddSeconds(ts);
    }
}
