using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bitsbybeier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class CmsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { message = "Welcome to the CMS", timestamp = DateTime.UtcNow });
    }

    [HttpGet("content")]
    public IActionResult GetContent()
    {
        // Placeholder for content management functionality
        var content = new[]
        {
            new { id = 1, title = "Sample Content 1", createdAt = DateTime.UtcNow.AddDays(-1) },
            new { id = 2, title = "Sample Content 2", createdAt = DateTime.UtcNow.AddDays(-2) }
        };

        return Ok(content);
    }

    [HttpPost("content")]
    public IActionResult CreateContent([FromBody] ContentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Title is required" });
        }

        // Placeholder for content creation
        var newContent = new
        {
            id = Random.Shared.Next(100, 999),
            title = request.Title,
            body = request.Body,
            createdAt = DateTime.UtcNow
        };

        return Created($"/api/cms/content/{newContent.id}", newContent);
    }
}

public record ContentRequest(string Title, string? Body);
