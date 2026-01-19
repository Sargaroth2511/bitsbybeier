using Microsoft.AspNetCore.Mvc;
using bitsbybeier.Api.Mcp;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Debug controller for MCP server
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class McpDebugController : ControllerBase
{
    private readonly ILogger<McpDebugController> _logger;
    private readonly ContentMcpTools? _mcpTools;

    public McpDebugController(ILogger<McpDebugController> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        // Try to resolve MCP tools
        _mcpTools = serviceProvider.GetService<ContentMcpTools>();
    }

    /// <summary>
    /// Check if MCP tools are registered
    /// </summary>
    [HttpGet("tools")]
    public IActionResult ListTools()
    {
        try
        {
            if (_mcpTools == null)
            {
                return Ok(new 
                { 
                    registered = false,
                    message = "ContentMcpTools not found in DI container",
                    note = "MCP tools should be registered via .WithTools<ContentMcpTools>()"
                });
            }

            return Ok(new
            {
                registered = true,
                message = "ContentMcpTools is registered",
                expectedTools = new[]
                {
                    "create_content",
                    "upload_image", 
                    "import_image_from_url",
                    "generate_image"
                },
                note = "Use MCP protocol POST /api/mcp with method 'tools/list' to query available tools"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking MCP tools");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
