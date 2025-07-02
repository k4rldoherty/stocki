namespace Stocki.Application.Exceptions;

public class ExternalServiceException : Exception
{
    public string UserFriendlyMessage { get; }

    public ExternalServiceException(
        string userFriendlyMessage,
        string message,
        Exception? innerException = null
    )
        : base(message: message, innerException: innerException)
    {
        UserFriendlyMessage = userFriendlyMessage;
    }
}
