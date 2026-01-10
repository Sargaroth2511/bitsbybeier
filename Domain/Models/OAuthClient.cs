using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents an OAuth 2.0 client application that can access the MCP server.
/// </summary>
public class OAuthClient
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string ClientSecret { get; set; } = string.Empty; // Stored hashed
    
    [Required]
    [MaxLength(200)]
    public string ClientName { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON array of allowed redirect URIs
    /// </summary>
    [Required]
    public string RedirectUris { get; set; } = "[]";
    
    /// <summary>
    /// JSON array of allowed scopes (e.g., ["mcp:read", "mcp:write"])
    /// </summary>
    [Required]
    public string AllowedScopes { get; set; } = "[]";
    
    public bool Active { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int? CreatedByUserId { get; set; }
    
    public User? CreatedBy { get; set; }
}
