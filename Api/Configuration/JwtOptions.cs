namespace bitsbybeier.Api.Configuration;

/// <summary>
/// Configuration options for JWT token generation and validation.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Authentication:Jwt";

    /// <summary>
    /// Secret key used for signing JWT tokens.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Issuer claim for JWT tokens.
    /// </summary>
    public string Issuer { get; set; } = "bitsbybeier";

    /// <summary>
    /// Audience claim for JWT tokens.
    /// </summary>
    public string Audience { get; set; } = "bitsbybeier-app";

    /// <summary>
    /// Token expiration time in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}
