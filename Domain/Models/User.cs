using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents a user in the system with authentication and profile information.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// User's email address (unique).
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's display name shown in the UI.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Google account identifier for OAuth authentication.
    /// </summary>
    [MaxLength(255)]
    public string? GoogleId { get; set; }
    
    /// <summary>
    /// Foreign key to the user's profile image.
    /// </summary>
    public int? ProfileImageId { get; set; }
    
    /// <summary>
    /// Navigation property to the user's profile image.
    /// </summary>
    public UserImage? ProfileImage { get; set; }
    
    /// <summary>
    /// UTC timestamp when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// UTC timestamp of the user's last successful login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Indicates whether the user account is active and can authenticate.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Indicates whether the user account has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// User's role determining access permissions.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;
}
