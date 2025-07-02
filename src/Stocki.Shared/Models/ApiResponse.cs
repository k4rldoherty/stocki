using System.Net;

namespace Stocki.Shared.Models;

/// <summary>
/// Represents a standardized response from an API operation, encapsulating success/failure and data.
/// </summary>
/// <typeparam name="T">The type of data returned on success.</typeparam>
public class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public HttpStatusCode StatusCode { get; init; }

    // Private constructor for successful responses with data
    private ApiResponse(T data, HttpStatusCode statusCode, string? message = null)
    {
        IsSuccess = true;
        Data = data;
        StatusCode = statusCode;
        Message = message;
    }

    // Private constructor for failed responses or successful responses without data
    private ApiResponse(bool isSuccess, HttpStatusCode statusCode, string? message)
    {
        IsSuccess = isSuccess;
        Data = default;
        StatusCode = statusCode;
        Message = message;
    }

    /// <summary>
    /// Creates a successful API response with data.
    /// </summary>
    /// <param name="data">The data payload of the response.</param>
    /// <param name="statusCode">The HTTP status code. Defaults to OK (200).</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>A new <see cref="ApiResponse{T}"/> instance indicating success.</returns>
    public static ApiResponse<T> Success(
        T data,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string? message = null
    )
    {
        return new ApiResponse<T>(data, statusCode, message);
    }

    /// <summary>
    /// Creates a successful API response without a specific data payload (e.g., for void operations).
    /// </summary>
    /// <param name="statusCode">The HTTP status code. Defaults to OK (200).</param>
    /// <param name="message">An optional success message.</param>
    /// <returns>A new <see cref="ApiResponse{T}"/> instance indicating success.</returns>
    public static ApiResponse<T> Success(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string? message = null
    )
    {
        return new ApiResponse<T>(true, statusCode, message);
    }

    /// <summary>
    /// Creates a failed API response with an error message and optional status code and exception.
    /// </summary>
    /// <param name="message">A descriptive error message.</param>
    /// <param name="statusCode">The HTTP status code. Defaults to InternalServerError (500) if not specified.</param>
    /// <returns>A new <see cref="ApiResponse{T}"/> instance indicating failure.</returns>
    public static ApiResponse<T> Failure(
        string message,
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError
    )
    {
        return new ApiResponse<T>(false, statusCode, message);
    }
}
