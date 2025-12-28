using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Interface for content management operations.
/// </summary>
public interface IContentService
{
    /// <summary>
    /// Creates a new content item.
    /// </summary>
    /// <param name="author">Author of the content.</param>
    /// <param name="title">Title of the content.</param>
    /// <param name="subtitle">Optional subtitle of the content.</param>
    /// <param name="contentText">Main content text (supports Markdown).</param>
    /// <param name="draft">Whether the content should be created as a draft (default: true).</param>
    /// <returns>The created content item.</returns>
    Task<Content> CreateContentAsync(string author, string title, string? subtitle, string contentText, bool draft = true);
}
