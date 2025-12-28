using Microsoft.EntityFrameworkCore;
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
    /// <param name="author">Author of the content.</param>
    /// <param name="title">Title of the content.</param>
    /// <param name="subtitle">Optional subtitle of the content.</param>
    /// <param name="contentText">Main content text (supports Markdown).</param>
    /// <param name="draft">Whether the content should be created as a draft (default: true).</param>
    /// <returns>The created content item.</returns>
    public async Task<Content> CreateContentAsync(string author, string title, string? subtitle, string contentText, bool draft = true)
    {
        _logger.LogInformation("Creating new content with title: {Title}, draft: {Draft}", title, draft);

        var content = new Content
        {
            Author = author,
            Title = title,
            Subtitle = subtitle,
            ContentText = contentText,
            Draft = draft,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contents.Add(content);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Content created successfully with ID: {ContentId}", content.Id);

        return content;
    }
}
