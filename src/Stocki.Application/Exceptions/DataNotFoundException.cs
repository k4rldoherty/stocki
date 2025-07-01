namespace Stocki.Application.Exceptions;

public class StockDataNotFoundException : Exception
{
    public string UserFriendlyMessage { get; }

    public StockDataNotFoundException(
        string message,
        string userFriendlyMessage,
        Exception? innerException = null
    )
        : base(message, innerException)
    {
        UserFriendlyMessage = userFriendlyMessage;
    }
}
