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
    private readonly IUrlValidationService _urlValidation;

    /// <summary>
    /// Initializes a new instance of the ContentService.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="urlValidation">URL validation service.</param>
    public ContentService(
        ApplicationDbContext context, 
        ILogger<ContentService> logger,
        IUrlValidationService urlValidation)
    {
        _context = context;
        _logger = logger;
        _urlValidation = urlValidation;
    }

    /// <summary>
    /// Creates a new content item.
    /// </summary>
    /// <param name="request">Content creation request containing all necessary fields.</param>
    /// <returns>The created content item.</returns>
    public async Task<Content> CreateContentAsync(ContentRequest request)
    {
        _logger.LogInformation("Creating new content with title: {Title}, draft: {Draft}", request.Title, request.Draft);

        // Sanitize content to remove malicious URLs
        var sanitizedContent = _urlValidation.SanitizeMarkdown(request.Content);
        if (sanitizedContent != request.Content)
        {
            _logger.LogWarning("Content contained unsafe URLs that were sanitized");
        }

        // Check for duplicate content created in the last 30 seconds to prevent double-creation from MCP client retries
        var recentDuplicate = await _context.Contents
            .Where(c => c.Title == request.Title 
                     && c.Author == request.Author 
                     && c.CreatedAt > DateTime.UtcNow.AddSeconds(-30))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (recentDuplicate != null)
        {
            _logger.LogWarning("Duplicate content detected within 30 seconds. Returning existing content ID: {ContentId}", recentDuplicate.Id);
            return recentDuplicate;
        }

        var content = new Content
        {
            Author = request.Author,
            Title = request.Title,
            Subtitle = request.Subtitle,
            ContentText = sanitizedContent,
            Draft = request.Draft,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        // Attach images if provided
        if (request.ImageIds != null && request.ImageIds.Count > 0)
        {
            await AttachImagesToContentAsync(content.Id, request.ImageIds);
        }

        _logger.LogInformation("Content created successfully with ID: {ContentId}", content.Id);

        return content;
    }
    
    /// <summary>
    /// Attaches multiple images to a content item.
    /// </summary>
    private async Task AttachImagesToContentAsync(int contentId, List<int> imageIds)
    {
        var images = await _context.ContentImages
            .Where(i => imageIds.Contains(i.Id))
            .ToListAsync();
            
        if (images.Count != imageIds.Count)
        {
            var foundIds = images.Select(i => i.Id).ToList();
            var missingIds = imageIds.Except(foundIds).ToList();
            _logger.LogWarning("Some image IDs were not found: {MissingIds}", string.Join(", ", missingIds));
        }
        
        foreach (var image in images)
        {
            image.ContentId = contentId;
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Attached {Count} images to content {ContentId}", images.Count, contentId);
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
