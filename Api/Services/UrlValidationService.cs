using System.Text.RegularExpressions;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for validating and sanitizing URLs to prevent XSS and malicious content injection.
/// </summary>
public class UrlValidationService : IUrlValidationService
{
    private readonly ILogger<UrlValidationService> _logger;
    
    // Allowed URL schemes - only safe protocols
    private static readonly HashSet<string> AllowedSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "http",
        "https",
        "mailto"
    };
    
    // Blocked patterns that might indicate XSS attempts
    private static readonly Regex DangerousPatterns = new(
        @"javascript:|data:|vbscript:|file:|about:|<script|onerror=|onclick=|onload=",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );
    
    public UrlValidationService(ILogger<UrlValidationService> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc/>
    public bool IsUrlSafe(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }
        
        // Check for dangerous patterns first
        if (DangerousPatterns.IsMatch(url))
        {
            _logger.LogWarning("Blocked dangerous pattern in URL: {Url}", url);
            return false;
        }
        
        // Try to parse the URL
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            // Allow relative URLs (e.g., /article/123)
            if (Uri.TryCreate(url, UriKind.Relative, out _))
            {
                return !DangerousPatterns.IsMatch(url);
            }
            
            _logger.LogWarning("Invalid URL format: {Url}", url);
            return false;
        }
        
        // Check if scheme is allowed
        if (!AllowedSchemes.Contains(uri.Scheme))
        {
            _logger.LogWarning("Blocked disallowed URL scheme: {Scheme} in {Url}", uri.Scheme, url);
            return false;
        }
        
        return true;
    }
    
    /// <inheritdoc/>
    public string SanitizeMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return markdown;
        }
        
        // Pattern to match Markdown links: [text](url) and ![alt](url)
        var linkPattern = new Regex(@"(!?\[([^\]]+)\])\(([^\)]+)\)", RegexOptions.Compiled);
        
        var sanitized = linkPattern.Replace(markdown, match =>
        {
            var fullMatch = match.Groups[0].Value; // Full match: [text](url) or ![alt](url)
            var linkPart = match.Groups[1].Value;  // [text] or ![alt]
            var url = match.Groups[3].Value.Trim();
            
            if (!IsUrlSafe(url))
            {
                _logger.LogWarning("Removed unsafe URL from markdown: {Url}", url);
                // Remove the URL but keep the text/alt
                return match.Groups[2].Value; // Just the text without link
            }
            
            return fullMatch;
        });
        
        // Pattern to match HTML links: <a href="url">
        var htmlLinkPattern = new Regex(@"<a\s+[^>]*href\s*=\s*[""']([^""']+)[""'][^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        
        sanitized = htmlLinkPattern.Replace(sanitized, match =>
        {
            var url = match.Groups[1].Value.Trim();
            
            if (!IsUrlSafe(url))
            {
                _logger.LogWarning("Removed unsafe href from HTML link: {Url}", url);
                return ""; // Remove the entire link tag
            }
            
            return match.Value;
        });
        
        return sanitized;
    }
}
