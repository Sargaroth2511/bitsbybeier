using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents a content item in the CMS with support for drafts and publication workflow.
/// </summary>
public class Content
{
    /// <summary>
    /// Unique identifier for the content item.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Author of the content.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Author { get; set; } = string.Empty;
    
    /// <summary>
    /// UTC timestamp when the content was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// UTC timestamp when the content was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Indicates whether the content is active.
    /// </summary>
    public bool Active { get; set; } = true;
    
    /// <summary>
    /// Indicates whether the content is in draft state.
    /// Draft content is not published and can be used for preview purposes.
    /// </summary>
    public bool Draft { get; set; } = true;
    
    /// <summary>
    /// Title of the content item.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Subtitle or summary of the content item.
    /// </summary>
    [MaxLength(1000)]
    public string? Subtitle { get; set; }
    
    /// <summary>
    /// Main content text, supports Markdown formatting.
    /// </summary>
    [Required]
    public string ContentText { get; set; } = string.Empty;
    
    /// <summary>
    /// UTC timestamp when the content should be published.
    /// If null, content can be published immediately when Draft is set to false.
    /// </summary>
    public DateTime? PublishAt { get; set; }
    
    /// <summary>
    /// Navigation property for associated images.
    /// </summary>
    public List<ContentImage> Images { get; set; } = new();
}
