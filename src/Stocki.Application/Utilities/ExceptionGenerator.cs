using System;
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
                    baseLogMessage,
                    "Oops! I'm getting too many requests right now. Please try again in a minute.",
                    innerException
                );

            case HttpStatusCode.NotFound:
                if (
                    !string.IsNullOrWhiteSpace(identifier)
                    && !commandType.Contains("General", StringComparison.OrdinalIgnoreCase)
                )
                {
                    return new StockDataNotFoundException(
                        baseLogMessage,
                        $"Sorry, I couldn't find any data for '{identifier}'. Please check if it's correct.",
                        innerException
                    );
                }
                else
                {
                    return new ExternalServiceException(
                        baseLogMessage,
                        "The requested information could not be found. It might be invalid or no longer exists.",
                        innerException
                    );
                }

            case HttpStatusCode.BadRequest:
                return new ExternalServiceException(
                    baseLogMessage,
                    "There was a problem with my request to the service. Please try again or contact support.",
                    innerException
                );

            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                return new ExternalServiceException(
                    baseLogMessage,
                    "I'm having trouble connecting to the external service. This is likely a configuration issue on my end.",
                    innerException
                );

            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.BadGateway:
            case HttpStatusCode.GatewayTimeout:
                return new ExternalServiceException(
                    baseLogMessage,
                    "The external service is currently unavailable or unresponsive. Please try again in a few minutes.",
                    innerException
                );

            case HttpStatusCode.InternalServerError:
                return new ExternalServiceException(
                    baseLogMessage,
                    "An unexpected error occurred with the external service. Please try again later.",
                    innerException
                );

            default:
                return new ExternalServiceException(
                    baseLogMessage,
                    "An unexpected problem occurred while processing your request. I'll let my developer know!",
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
            baseLogMessage,
            $"Sorry, I couldn't find any data for '{identifier}'. {technicalReason}. Please check your input or try again.",
            innerException
        );
    }
}
