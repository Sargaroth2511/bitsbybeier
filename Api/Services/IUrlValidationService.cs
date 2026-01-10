namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for validating and sanitizing URLs to prevent malicious content.
/// </summary>
public interface IUrlValidationService
{
    /// <summary>
    /// Validates if a URL is safe and properly formed.
    /// Blocks javascript:, data:, file:, and other dangerous protocols.
    /// </summary>
    /// <param name="url">URL to validate.</param>
    /// <returns>True if URL is safe, false otherwise.</returns>
    bool IsUrlSafe(string url);
    
    /// <summary>
    /// Sanitizes Markdown content by validating all URLs.
    /// </summary>
    /// <param name="markdown">Markdown content to sanitize.</param>
    /// <returns>Sanitized markdown with validated URLs.</returns>
    string SanitizeMarkdown(string markdown);
}
