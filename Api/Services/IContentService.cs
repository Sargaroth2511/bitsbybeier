using bitsbybeier.Api.Models;
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
    /// <param name="request">Content creation request containing all necessary fields.</param>
    /// <returns>The created content item.</returns>
    Task<Content> CreateContentAsync(ContentRequest request);

    /// <summary>
    /// Updates an existing content item's status.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <param name="request">Update request with fields to update.</param>
    /// <returns>The updated content item.</returns>
    Task<Content> UpdateContentAsync(int id, ContentUpdateRequest request);

    /// <summary>
    /// Updates an existing content item with full content fields.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <param name="request">Full update request with all fields to update.</param>
    /// <returns>The updated content item.</returns>
    Task<Content> UpdateContentFullAsync(int id, ContentFullUpdateRequest request);

    /// <summary>
    /// Deletes a content item.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <returns>True if deleted successfully.</returns>
    Task<bool> DeleteContentAsync(int id);

    /// <summary>
    /// Gets a content item by ID.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <returns>The content item or null if not found.</returns>
    Task<Content?> GetContentByIdAsync(int id);
}
