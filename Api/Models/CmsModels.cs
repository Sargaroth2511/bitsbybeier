using System.ComponentModel.DataAnnotations;

namespace bitsbybeier.Api.Models;

/// <summary>
/// Request model for creating content in CMS.
/// </summary>
public record ContentRequest
{
    /// <summary>
    /// Author of the content (required).
    /// </summary>
    [Required(ErrorMessage = "Author is required")]
    [MaxLength(200, ErrorMessage = "Author must not exceed 200 characters")]
    public required string Author { get; init; }

    /// <summary>
    /// Title of the content (required).
    /// </summary>
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(500, ErrorMessage = "Title must not exceed 500 characters")]
    public required string Title { get; init; }

    /// <summary>
    /// Subtitle or summary of the content (optional).
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Subtitle must not exceed 1000 characters")]
    public string? Subtitle { get; init; }

    /// <summary>
    /// Main content text, supports Markdown (required).
    /// </summary>
    [Required(ErrorMessage = "Content is required")]
    public required string Content { get; init; }

    /// <summary>
    /// Whether the content should be created as a draft (default: true).
    /// </summary>
    public bool Draft { get; init; } = true;
}

/// <summary>
/// Response model for content items.
/// </summary>
public record ContentResponse
{
    /// <summary>
    /// Unique identifier for the content.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    /// Author of the content.
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    /// Content title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Subtitle or summary of the content.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// Main content text.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Whether the content is in draft state.
    /// </summary>
    public required bool Draft { get; init; }

    /// <summary>
    /// Whether the content is active.
    /// </summary>
    public required bool Active { get; init; }

    /// <summary>
    /// Timestamp when content was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when content was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
