using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

public class UserImage
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public byte[] ImageData { get; set; } = Array.Empty<byte>();
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty; // e.g., "image/jpeg", "image/png"
    
    [MaxLength(255)]
    public string? FileName { get; set; }
    
    public long FileSize { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
