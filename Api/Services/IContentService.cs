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
}
