namespace bitsbybeier.Api.Models;

/// <summary>
/// Response model for successful authentication.
/// </summary>
public record AuthenticationResponse
{
    /// <summary>
    /// JWT token for authenticated requests.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's display name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// User's role in the system.
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public required int UserId { get; init; }
}

/// <summary>
/// Request model for Google token authentication.
/// </summary>
public record GoogleTokenRequest
{
    /// <summary>
    /// Google ID token from client-side authentication.
    /// </summary>
    public required string IdToken { get; init; }
}

/// <summary>
/// Response model for user information.
/// </summary>
public record UserResponse
{
    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's display name.
    /// </summary>
    public required string Name { get; init; }
}
