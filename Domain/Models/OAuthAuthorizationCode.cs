using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents a short-lived authorization code for OAuth 2.0 flow.
/// </summary>
public class OAuthAuthorizationCode
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string RedirectUri { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Scope { get; set; } = string.Empty;
    
    /// <summary>
    /// PKCE code challenge (optional)
    /// </summary>
    [MaxLength(100)]
    public string? CodeChallenge { get; set; }
    
    /// <summary>
    /// PKCE challenge method (S256 or plain)
    /// </summary>
    [MaxLength(10)]
    public string? CodeChallengeMethod { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public bool Used { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
