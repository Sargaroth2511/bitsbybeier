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
}
