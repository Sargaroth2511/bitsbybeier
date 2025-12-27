using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using bitsbybeier.Api.Models;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Controller for Content Management System (CMS) operations.
/// Accessible only to users with Admin role.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CmsController : ControllerBase
{
    private readonly ILogger<CmsController> _logger;

    /// <summary>
    /// Initializes a new instance of the CmsController.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public CmsController(ILogger<CmsController> logger)
    {
        _logger = logger;
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
        _logger.LogInformation("CMS root endpoint accessed");
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
    public IActionResult GetContent()
    {
        // Placeholder for content management functionality
        var content = new[]
        {
            new ContentResponse 
            { 
                Id = 1, 
                Title = "Sample Content 1", 
                Body = null, 
                CreatedAt = DateTime.UtcNow.AddDays(-1) 
            },
            new ContentResponse 
            { 
                Id = 2, 
                Title = "Sample Content 2", 
                Body = null, 
                CreatedAt = DateTime.UtcNow.AddDays(-2) 
            }
        };

        _logger.LogDebug("Retrieved {Count} content items", content.Length);
        return Ok(content);
    }

    /// <summary>
    /// Creates a new content item.
    /// </summary>
    /// <param name="request">Content creation request with title and optional body.</param>
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
    public IActionResult CreateContent([FromBody] ContentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new ErrorResponse { Message = "Title is required" });
        }

        // Placeholder for content creation
        var newContent = new ContentResponse
        {
            Id = Random.Shared.Next(100, 999),
            Title = request.Title,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Content created with ID {ContentId}", newContent.Id);
        return Created($"/api/cms/content/{newContent.Id}", newContent);
    }
}
