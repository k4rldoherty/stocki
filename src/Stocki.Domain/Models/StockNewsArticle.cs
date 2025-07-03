namespace Stocki.Domain.Models;

public class StockNewsArticle
{
    public DateTime DateOfArticle;
    public String Headline = string.Empty;
    public String? ImageUrl;
    public String Source = string.Empty;
    public String Summary = string.Empty;
    public String Url = string.Empty;

    public StockNewsArticle() { }

    public StockNewsArticle(
        String timeStamp,
        String headline,
        String imageUrl,
        String source,
        String summary,
        String url
    )
    {
        DateOfArticle = ConvertTimestampToDatetime(timeStamp);
        Headline = headline;
        ImageUrl = imageUrl;
        Source = source;
        Summary = summary.Length > 200 ? summary.Substring(0, 100) + "..." : summary;
        Url = url;
    }

    private DateTime ConvertTimestampToDatetime(string ts)
    {
        if (double.TryParse(ts, out var timeStamp))
        {
            return DateTime.UnixEpoch.AddSeconds(timeStamp);
        }
        // Not perfect but will do for now
        return DateTime.Now;
    }
}
