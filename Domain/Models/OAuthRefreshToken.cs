using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents a refresh token for OAuth 2.0 token renewal.
/// </summary>
public class OAuthRefreshToken
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Scope { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool Revoked { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
