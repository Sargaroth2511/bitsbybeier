using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace bitsbybeier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CmsController : ControllerBase
{
    private readonly ILogger<CmsController> _logger;

    public CmsController(ILogger<CmsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("content")]
    public IActionResult GetContent()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;

        _logger.LogInformation("User {Email} accessed CMS content", email);

        return Ok(new
        {
            message = "Welcome to the CMS",
            user = new { email, name },
            content = new[]
            {
                new { id = 1, title = "Sample Content 1", body = "This is sample content from the CMS" },
                new { id = 2, title = "Sample Content 2", body = "This is another sample content" }
            }
        });
    }

    [HttpPost("content")]
    public IActionResult CreateContent([FromBody] ContentRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        _logger.LogInformation("User {Email} created content: {Title}", email, request.Title);

        return Ok(new
        {
            message = "Content created successfully",
            content = new { id = 3, title = request.Title, body = request.Body }
        });
    }

    [HttpPut("content/{id}")]
    public IActionResult UpdateContent(int id, [FromBody] ContentRequest request)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        _logger.LogInformation("User {Email} updated content {Id}: {Title}", email, id, request.Title);

        return Ok(new
        {
            message = "Content updated successfully",
            content = new { id, title = request.Title, body = request.Body }
        });
    }

    [HttpDelete("content/{id}")]
    public IActionResult DeleteContent(int id)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        _logger.LogInformation("User {Email} deleted content {Id}", email, id);

        return Ok(new { message = "Content deleted successfully" });
    }
}

public class ContentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
