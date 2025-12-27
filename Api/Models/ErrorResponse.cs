namespace bitsbybeier.Api.Models;

/// <summary>
/// Standard error response model for API errors.
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// Error message describing what went wrong.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional error code for client-side error handling.
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// Optional detailed error information (for debugging).
    /// </summary>
    public object? Details { get; init; }
}
