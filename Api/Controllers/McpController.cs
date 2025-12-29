using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol.Server;
using bitsbybeier.Api.Mcp;
using bitsbybeier.Data;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Controller for Model Context Protocol (MCP) operations.
/// Provides endpoints for AI agents to discover and invoke tools.
/// Requires authentication and Admin role for security.
/// </summary>
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class McpController : BaseController
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the McpController.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="serviceProvider">Service provider for resolving MCP tools.</param>
    /// <param name="context">Database context.</param>
    public McpController(ILogger<McpController> logger, IServiceProvider serviceProvider, ApplicationDbContext context)
        : base(logger, context)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets information about the MCP server and available tools.
    /// </summary>
    /// <returns>Server information and tool list.</returns>
    /// <response code="200">Returns the MCP server information.</response>
    /// <response code="401">If user is not authenticated.</response>
    /// <response code="403">If user does not have Admin role.</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetServerInfo()
    {
        Logger.LogInformation("MCP server info requested by user: {UserId}", UserId);
        
        return Ok(new 
        { 
            name = "BitsbyBeier Content MCP Server",
            version = "1.0.0",
            description = "MCP server for content management operations. Provides AI agents with the ability to create content items programmatically. Requires Admin role.",
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
                                author = new 
                                { 
                                    type = "string", 
                                    required = true, 
                                    maxLength = 200,
                                    description = "The author name of the content. Maximum 200 characters." 
                                },
                                title = new 
                                { 
                                    type = "string", 
                                    required = true, 
                                    maxLength = 500,
                                    description = "The title of the content. Maximum 500 characters." 
                                },
                                content = new 
                                { 
                                    type = "string", 
                                    required = true, 
                                    description = "The main content text. Supports Markdown formatting for rich text content. No length limit." 
                                },
                                subtitle = new 
                                { 
                                    type = "string", 
                                    required = false, 
                                    maxLength = 1000,
                                    description = "Optional subtitle or summary of the content. Maximum 1000 characters." 
                                },
                                draft = new 
                                { 
                                    type = "boolean", 
                                    required = false, 
                                    @default = true, 
                                    description = "Whether to create as draft (default: true). Draft content is not published and can be used for preview." 
                                }
                            }
                        }
                    }
                }
            }
        });
    }
}
