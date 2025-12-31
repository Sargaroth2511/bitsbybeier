using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bitsbybeier.Api.Models;
using bitsbybeier.Api.Services;
using bitsbybeier.Data;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Controller for Content Management System (CMS) operations.
/// Accessible only to users with Admin role.
/// </summary>
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CmsController : BaseController
{
    private readonly IContentService _contentService;

    /// <summary>
    /// Initializes a new instance of the CmsController.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="contentService">Content service.</param>
    /// <param name="context">Database context.</param>
    public CmsController(ILogger<CmsController> logger, IContentService contentService, ApplicationDbContext context)
        : base(logger, context)
    {
        _contentService = contentService;
    }

    /// <summary>
    /// Gets CMS welcome message and timestamp.
    /// </summary>
    /// <returns>Welcome message with current timestamp.</returns>
    /// <response code="200">Returns the welcome message.</response>
    /// <response code="401">If user is not authenticated.</response>
    /// <response code="403">If user does not have Admin role.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Get()
    {
        Logger.LogInformation("CMS root endpoint accessed");
        return Ok(new { message = "Welcome to the CMS", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Gets a list of all content items.
    /// </summary>
    /// <returns>Array of content items.</returns>
    /// <response code="200">Returns the list of content items.</response>
    /// <response code="401">If user is not authenticated.</response>
    /// <response code="403">If user does not have Admin role.</response>
    [HttpGet("content")]
    [ProducesResponseType(typeof(IEnumerable<ContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetContent()
    {
        var contents = await Context.Contents
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var response = contents.Select(c => new ContentResponse
        {
            Id = c.Id,
            Author = c.Author,
            Title = c.Title,
            Subtitle = c.Subtitle,
            Content = c.ContentText,
            Draft = c.Draft,
            Active = c.Active,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            PublishAt = c.PublishAt
        });

        Logger.LogDebug("Retrieved {Count} content items", contents.Count);
        return Ok(response);
    }

    /// <summary>
    /// Gets a list of draft content items only.
    /// </summary>
    /// <returns>Array of draft content items.</returns>
    /// <response code="200">Returns the list of draft content items.</response>
    /// <response code="401">If user is not authenticated.</response>
    /// <response code="403">If user does not have Admin role.</response>
    [HttpGet("content/drafts")]
    [ProducesResponseType(typeof(IEnumerable<ContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDraftContent()
    {
        var contents = await Context.Contents
            .Where(c => c.Draft)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var response = contents.Select(c => new ContentResponse
        {
            Id = c.Id,
            Author = c.Author,
            Title = c.Title,
            Subtitle = c.Subtitle,
            Content = c.ContentText,
            Draft = c.Draft,
            Active = c.Active,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            PublishAt = c.PublishAt
        });

        Logger.LogDebug("Retrieved {Count} draft content items", contents.Count);
        return Ok(response);
    }

    /// <summary>
    /// Gets a list of public (non-draft) content items.
    /// </summary>
    /// <returns>Array of public content items.</returns>
    /// <response code="200">Returns the list of public content items.</response>
    [HttpGet("content/public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ContentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicContent()
    {
        var contents = await Context.Contents
            .Where(c => !c.Draft && c.Active)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var response = contents.Select(c => new ContentResponse
        {
            Id = c.Id,
            Author = c.Author,
            Title = c.Title,
            Subtitle = c.Subtitle,
            Content = c.ContentText,
            Draft = c.Draft,
            Active = c.Active,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            PublishAt = c.PublishAt
        });

        Logger.LogDebug("Retrieved {Count} public content items", contents.Count);
        return Ok(response);
    }

    /// <summary>
    /// Creates a new content item.
    /// </summary>
    /// <param name="request">Content creation request with all required fields.</param>
    /// <returns>The created content item.</returns>
    /// <response code="201">Returns the newly created content item.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If user is not authenticated.</response>
    /// <response code="403">If user does not have Admin role.</response>
    [HttpPost("content")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateContent(ContentRequest request)
    {
        var content = await _contentService.CreateContentAsync(request);

        var response = new ContentResponse
        {
            Id = content.Id,
            Author = content.Author,
            Title = content.Title,
            Subtitle = content.Subtitle,
            Content = content.ContentText,
            Draft = content.Draft,
            Active = content.Active,
            CreatedAt = content.CreatedAt,
            UpdatedAt = content.UpdatedAt,
            PublishAt = content.PublishAt
        };

        Logger.LogInformation("Content created with ID {ContentId}", content.Id);
        return Created($"/api/cms/content/{content.Id}", response);
    }

    /// <summary>
    /// Updates an existing content item's status.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <param name="request">Update request with fields to update.</param>
    /// <returns>The updated content item.</returns>
    /// <response code="200">Returns the updated content item.</response>
    /// <response code="400">If the request data is invalid.</response>
    /// <response code="401">If user is not authenticated.</response>
    /// <response code="403">If user does not have Admin role.</response>
    /// <response code="404">If content item not found.</response>
    [HttpPut("content/{id}")]
    [ProducesResponseType(typeof(ContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContent(int id, ContentUpdateRequest request)
    {
        try
        {
            var content = await _contentService.UpdateContentAsync(id, request);

            var response = new ContentResponse
            {
                Id = content.Id,
                Author = content.Author,
                Title = content.Title,
                Subtitle = content.Subtitle,
                Content = content.ContentText,
                Draft = content.Draft,
                Active = content.Active,
                CreatedAt = content.CreatedAt,
                UpdatedAt = content.UpdatedAt,
                PublishAt = content.PublishAt
            };

            Logger.LogInformation("Content updated with ID {ContentId}", content.Id);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            Logger.LogWarning("Content not found: {Message}", ex.Message);
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a content item.
    /// </summary>
    /// <param name="id">Content item ID.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Content deleted successfully.</response>
    /// <response code="401">If user is not authenticated.</response>
    /// <response code="403">If user does not have Admin role.</response>
    /// <response code="404">If content item not found.</response>
    [HttpDelete("content/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContent(int id)
    {
        var deleted = await _contentService.DeleteContentAsync(id);
        
        if (!deleted)
        {
            Logger.LogWarning("Content not found with ID {ContentId}", id);
            return NotFound(new ErrorResponse { Message = $"Content with ID {id} not found" });
        }

        Logger.LogInformation("Content deleted with ID {ContentId}", id);
        return NoContent();
    }
}
