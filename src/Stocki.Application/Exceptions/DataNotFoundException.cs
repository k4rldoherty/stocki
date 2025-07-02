namespace Stocki.Application.Exceptions;

public class StockDataNotFoundException : Exception
{
    public string UserFriendlyMessage { get; }

    public StockDataNotFoundException(
        string userFriendlyMessage,
        string message,
        Exception? innerException = null
    )
        : base(message, innerException)
    {
        UserFriendlyMessage = userFriendlyMessage;
    }
}
