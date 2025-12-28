using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents an image associated with content, stored as a blob in the database.
/// </summary>
public class ContentImage
{
    /// <summary>
    /// Unique identifier for the image.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Binary image data.
    /// </summary>
    [Required]
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// MIME content type of the image (e.g., "image/jpeg", "image/png").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Original filename of the uploaded image.
    /// </summary>
    [MaxLength(255)]
    public string? FileName { get; set; }
    
    /// <summary>
    /// Size of the image file in bytes.
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// UTC timestamp when the image was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Foreign key to the associated content item.
    /// </summary>
    public int ContentId { get; set; }
    
    /// <summary>
    /// Navigation property to the associated content item.
    /// </summary>
    public Content Content { get; set; } = null!;
}
