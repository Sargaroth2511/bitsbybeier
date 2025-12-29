using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using bitsbybeier.Api.Mcp;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Controller for Model Context Protocol (MCP) operations.
/// Provides endpoints for AI agents to discover and invoke tools.
/// Requires authentication for security.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class McpController : BaseController
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the McpController.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="serviceProvider">Service provider for resolving MCP tools.</param>
    public McpController(ILogger<McpController> logger, IServiceProvider serviceProvider)
        : base(logger)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets information about the MCP server and available tools.
    /// </summary>
    /// <returns>Server information and tool list.</returns>
    /// <response code="200">Returns the MCP server information.</response>
    /// <response code="401">If user is not authenticated.</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetServerInfo()
    {
        Logger.LogInformation("MCP server info requested by user: {UserId}", UserId);
        
        return Ok(new 
        { 
            name = "BitsbyBeier Content MCP Server",
            version = "1.0.0",
            description = "MCP server for content management operations. Provides AI agents with the ability to create content items programmatically.",
            tools = new[]
            {
                new
                {
                    name = "CreateContentAsync",
                    description = "Creates a new content item in the CMS. The content is created as a draft by default and will not be published immediately. Use this tool to add new articles, blog posts, or any text-based content to the system.",
                    parameters = new
                    {
                        request = new 
                        { 
                            type = "object",
                            description = "Content creation request containing all necessary fields",
                            properties = new
                            {
                                author = new { type = "string", required = true, description = "The author name of the content" },
                                title = new { type = "string", required = true, description = "The title of the content" },
                                content = new { type = "string", required = true, description = "The main content text. Supports Markdown formatting for rich text content." },
                                subtitle = new { type = "string", required = false, description = "Optional subtitle or summary of the content" },
                                draft = new { type = "boolean", required = false, @default = true, description = "Whether to create as draft (default: true). Draft content is not published and can be used for preview." }
                            }
                        }
                    }
                }
            }
        });
    }
}
