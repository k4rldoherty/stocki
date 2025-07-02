namespace Stocki.Domain.Models;

public record StockNewsArticle
{
    public DateTime DateOfArticle;
    public String Headline = string.Empty;
    public String ImageUrl = string.Empty;
    public String Source = string.Empty;
    public String Summary = string.Empty;
    public String Url = string.Empty;

    public StockNewsArticle() { }

    public StockNewsArticle(String Ts, String H, String I, String S, String Summ, String U)
    {
        DateOfArticle = ConvertTimestampToDatetime(Ts);
        Headline = H;
        ImageUrl = I;
        Source = S;
        Summary = Summ.Length > 200 ? Summ.Substring(0, 100) + "..." : Summ;
        Url = U;
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
