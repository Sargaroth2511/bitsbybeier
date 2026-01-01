using Microsoft.EntityFrameworkCore;
using bitsbybeier.Api.Models;
using bitsbybeier.Data;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for content management operations.
/// </summary>
public class ContentService : IContentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ContentService> _logger;

    /// <summary>
    /// Initializes a new instance of the ContentService.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="logger">Logger instance.</param>
    public ContentService(ApplicationDbContext context, ILogger<ContentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new content item.
    /// </summary>
    /// <param name="request">Content creation request containing all necessary fields.</param>
    /// <returns>The created content item.</returns>
    public async Task<Content> CreateContentAsync(ContentRequest request)
    {
        _logger.LogInformation("Creating new content with title: {Title}, draft: {Draft}", request.Title, request.Draft);

        var content = new Content
        {
            Author = request.Author,
            Title = request.Title,
            Subtitle = request.Subtitle,
            ContentText = request.Content,
            Draft = request.Draft,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Content created successfully with ID: {ContentId}", content.Id);

        return content;
    }

    /// <summary>
    /// Updates an existing content item's status.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <param name="request">Update request with fields to update.</param>
    /// <returns>The updated content item.</returns>
    public async Task<Content> UpdateContentAsync(int id, ContentUpdateRequest request)
    {
        _logger.LogInformation("Updating content with ID: {ContentId}", id);

        var content = await _context.Contents.FindAsync(id);
        if (content == null)
        {
            throw new InvalidOperationException($"Content with ID {id} not found");
        }

        if (request.Draft.HasValue)
        {
            content.Draft = request.Draft.Value;
        }

        if (request.Active.HasValue)
        {
            content.Active = request.Active.Value;
        }

        if (request.PublishAt.HasValue)
        {
            content.PublishAt = request.PublishAt.Value;
        }

        content.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Content updated successfully with ID: {ContentId}", content.Id);

        return content;
    }

    /// <summary>
    /// Updates an existing content item with full content fields.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <param name="request">Full update request with all fields to update.</param>
    /// <returns>The updated content item.</returns>
    public async Task<Content> UpdateContentFullAsync(int id, ContentFullUpdateRequest request)
    {
        _logger.LogInformation("Fully updating content with ID: {ContentId}", id);

        var content = await _context.Contents.FindAsync(id);
        if (content == null)
        {
            throw new InvalidOperationException($"Content with ID {id} not found");
        }

        if (!string.IsNullOrEmpty(request.Author))
        {
            content.Author = request.Author;
        }

        if (!string.IsNullOrEmpty(request.Title))
        {
            content.Title = request.Title;
        }

        if (request.Subtitle != null)
        {
            content.Subtitle = request.Subtitle;
        }

        if (!string.IsNullOrEmpty(request.Content))
        {
            content.ContentText = request.Content;
        }

        if (request.Draft.HasValue)
        {
            content.Draft = request.Draft.Value;
        }

        if (request.Active.HasValue)
        {
            content.Active = request.Active.Value;
        }

        if (request.PublishAt.HasValue)
        {
            content.PublishAt = request.PublishAt.Value;
        }

        content.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Content fully updated successfully with ID: {ContentId}", content.Id);

        return content;
    }

    /// <summary>
    /// Deletes a content item.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <returns>True if deleted successfully.</returns>
    public async Task<bool> DeleteContentAsync(int id)
    {
        _logger.LogInformation("Deleting content with ID: {ContentId}", id);

        var content = await _context.Contents.FindAsync(id);
        if (content == null)
        {
            return false;
        }

        _context.Contents.Remove(content);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Content deleted successfully with ID: {ContentId}", id);

        return true;
    }

    /// <summary>
    /// Gets a content item by ID.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <returns>The content item or null if not found.</returns>
    public async Task<Content?> GetContentByIdAsync(int id)
    {
        return await _context.Contents.FindAsync(id);
    }
}
