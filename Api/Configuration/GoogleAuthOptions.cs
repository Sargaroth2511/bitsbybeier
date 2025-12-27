namespace bitsbybeier.Api.Configuration;

/// <summary>
/// Configuration options for Google OAuth authentication.
/// </summary>
public class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    /// <summary>
    /// Google OAuth Client ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Google OAuth Client Secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}
