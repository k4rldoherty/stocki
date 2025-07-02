using System.Net;
using Stocki.Application.Exceptions; // Assuming your custom exceptions are here

namespace Stocki.Application.Utilities; // Recommended location for such utilities

public static class ExceptionGenerator
{
    public static Exception GenerateNon200StatusCodeException(
        HttpStatusCode statusCode,
        string commandType, // e.g., "StockNewsQuery" or "StockOverviewQuery"
        string? identifier, // e.g., ticker symbol "AAPL", or category "general"
        string? apiResponseMessage = null,
        Exception? innerException = null
    )
    {
        string baseLogMessage =
            $"External API error during {commandType} for {identifier ?? "unknown"}. Status: {statusCode}. Original message: '{apiResponseMessage ?? "N/A"}'";

        switch (statusCode)
        {
            case HttpStatusCode.TooManyRequests:
                return new ExternalServiceException(
                    "Oops! I'm getting too many requests right now. Please try again in a minute.",
                    baseLogMessage,
                    innerException
                );

            case HttpStatusCode.NotFound:
                if (!string.IsNullOrWhiteSpace(identifier))
                {
                    return new StockDataNotFoundException(
                        $"Sorry, I couldn't find any data for '{identifier}'. Please check if it's correct.",
                        baseLogMessage,
                        innerException
                    );
                }
                else
                {
                    return new ExternalServiceException(
                        "The requested information could not be found. It might be invalid or no longer exists.",
                        baseLogMessage,
                        innerException
                    );
                }

            case HttpStatusCode.BadRequest:
                return new ExternalServiceException(
                    "There was a problem with my request to the service. Please try again or contact support.",
                    baseLogMessage,
                    innerException
                );

            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                return new ExternalServiceException(
                    "I'm having trouble connecting to the external service. This is likely a configuration issue on my end.",
                    baseLogMessage,
                    innerException
                );

            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.BadGateway:
            case HttpStatusCode.GatewayTimeout:
                return new ExternalServiceException(
                    "The external service is currently unavailable or unresponsive. Please try again in a few minutes.",
                    baseLogMessage,
                    innerException
                );

            case HttpStatusCode.InternalServerError:
                return new ExternalServiceException(
                    "An unexpected error occurred with the external service. Please try again later.",
                    baseLogMessage,
                    innerException
                );

            default:
                return new ExternalServiceException(
                    "An unexpected problem occurred while processing your request. I'll let my developer know!",
                    baseLogMessage,
                    innerException
                );
        }
    }

    public static Exception GenerateDataNotFoundException(
        string commandType,
        string? identifier,
        string technicalReason,
        Exception? innerException = null
    )
    {
        string baseLogMessage =
            $"Data not found during {commandType} for {identifier ?? "unknown"}. Technical reason: '{technicalReason}'.";

        return new StockDataNotFoundException(
            $"Sorry, I couldn't find any data for '{identifier}'. Please check your input or try again.",
            baseLogMessage,
            innerException
        );
    }
}
