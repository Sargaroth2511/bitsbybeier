using ModelContextProtocol.Server;
using bitsbybeier.Api.Models;
using bitsbybeier.Api.Services;

namespace bitsbybeier.Api.Mcp;

/// <summary>
/// MCP Server for content management operations.
/// Provides AI agents with the ability to create content items programmatically.
/// </summary>
public class ContentMcpTools
{
    private readonly IContentService _contentService;
    private readonly ILogger<ContentMcpTools> _logger;

    /// <summary>
    /// Initializes a new instance of the ContentMcpTools.
    /// </summary>
    /// <param name="contentService">Content service for content operations.</param>
    /// <param name="logger">Logger instance.</param>
    public ContentMcpTools(IContentService contentService, ILogger<ContentMcpTools> logger)
    {
        _contentService = contentService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new content item in the CMS. The content is created as a draft by default and will not be published immediately. 
    /// Use this tool to add new articles, blog posts, or any text-based content to the system.
    /// </summary>
    /// <param name="request">Content creation request containing all necessary fields including author, title, content, optional subtitle, and draft flag.</param>
    /// <returns>A message indicating the content was created successfully with details.</returns>
    [McpServerTool]
    public async Task<string> CreateContentAsync(ContentRequest request)
    {
        try
        {
            _logger.LogInformation("Creating content via MCP: {Title}, Draft: {Draft}", request.Title, request.Draft);

            var createdContent = await _contentService.CreateContentAsync(request);

            var result = $"Content created successfully. ID: {createdContent.Id}, Title: {createdContent.Title}, Draft: {createdContent.Draft}, Created: {createdContent.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC";
            _logger.LogInformation("Content created via MCP with ID: {ContentId}", createdContent.Id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating content via MCP");
            throw new InvalidOperationException($"Error creating content: {ex.Message}", ex);
        }
    }
}
