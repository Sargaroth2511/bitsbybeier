namespace bitsbybeier.Api.Models;

/// <summary>
/// Request model for creating content in CMS.
/// </summary>
public record ContentRequest
{
    /// <summary>
    /// Title of the content (required).
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Body content (optional).
    /// </summary>
    public string? Body { get; init; }
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
    /// Content title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Content body (optional).
    /// </summary>
    public string? Body { get; init; }

    /// <summary>
    /// Timestamp when content was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
