using ModelContextProtocol.Server;
using bitsbybeier.Api.Models;
using bitsbybeier.Api.Services;
using System.ComponentModel;

namespace bitsbybeier.Api.Mcp;

/// <summary>
/// MCP Server for content management operations.
/// Provides AI agents with the ability to create content items and manage images programmatically.
/// 
/// RECOMMENDED WORKFLOW FOR AI-GENERATED CONTENT WITH IMAGES:
/// 1. LLM calls generate_image tool with descriptive prompt → receives image ID
/// 2. Repeat step 1 for each image needed in the article
/// 3. LLM creates content text with Markdown syntax using image IDs: ![alt text](https://bitsbybeier.de/api/images/{imageId})
/// 4. LLM calls create_content with the content and comma-separated image IDs in imageIds parameter
/// 
/// EXAMPLE:
/// Step 1: Call generate_image("A chocolate cake with frosting") → Returns ID: 5
/// Step 2: Call generate_image("A bakery storefront") → Returns ID: 6
/// Step 3: Create content: "# Guide\n\n![Cake](https://bitsbybeier.de/api/images/5)\n\n![Store](https://bitsbybeier.de/api/images/6)"
/// Step 4: Call create_content with imageIds="5,6"
/// 
/// ALTERNATIVE WORKFLOW (for external images):
/// 1. Generate image with external tool (DALL-E, Midjourney, etc.) - receives a temporary URL
/// 2. Import image using import_image_from_url tool with the temporary URL (returns an image ID)
/// 3. Insert images into content text using Markdown syntax: ![alt text](https://bitsbybeier.de/api/images/{imageId})
/// 4. Attach images to content by providing comma-separated image IDs in the imageIds parameter
/// </summary>
[McpServerToolType]
public class ContentMcpTools
{
    private readonly IContentService _contentService;
    private readonly IImageService _imageService;
    private readonly IOpenAIImageService _openAIImageService;
    private readonly ILogger<ContentMcpTools> _logger;

    /// <summary>
    /// Initializes a new instance of the ContentMcpTools.
    /// </summary>
    /// <param name="contentService">Content service for content operations.</param>
    /// <param name="imageService">Image service for image operations.</param>
    /// <param name="openAIImageService">OpenAI image service for AI image generation.</param>
    /// <param name="logger">Logger instance.</param>
    public ContentMcpTools(
        IContentService contentService, 
        IImageService imageService,
        IOpenAIImageService openAIImageService,
        ILogger<ContentMcpTools> logger)
    {
        _contentService = contentService;
        _imageService = imageService;
        _openAIImageService = openAIImageService;
        _logger = logger;
        
        _logger.LogInformation("=== ContentMcpTools instance created ===");
        _logger.LogInformation("Available tools: create_content, upload_image, import_image_from_url, generate_image");
    }

    /// <summary>
    /// Creates a new content item in the CMS. Content is created as draft by default (not published).
    /// Use this to add articles, blog posts, or any text-based content.
    /// Content supports Markdown formatting, including links and references.
    /// 
    /// CHATGPT: HOW TO ADD DALL-E IMAGES (COMPLETE WORKFLOW):
    /// 
    /// STEP 1: Generate image with DALL-E
    ///   - You receive a URL like: https://oaidalleapiprodscus.blob.core.windows.net/private/.../img-xyz.png?se=...
    /// 
    /// STEP 2: Import the image using import_image_from_url tool
    ///   - Call: import_image_from_url(imageUrl="https://oaidalleapiprodscus.blob.core.windows.net/...", description="Chocolate cake")
    ///   - Response: "Image imported successfully. ID: 5"
    ///   - ✅ This works! No CORS issues, no restrictions on DALL-E URLs
    /// 
    /// STEP 3: Create content with the image
    ///   - In 'content' parameter, write: "Here is the cake: ![Chocolate Cake](https://bitsbybeier.de/api/images/5)"
    ///   - In 'imageIds' parameter, provide: "5" (or "5,7,9" for multiple images)
    ///   - The URL format MUST be: https://bitsbybeier.de/api/images/{ID from step 2}
    ///   - Alt text (between [ ]) describes the image
    /// 
    /// EXAMPLE COMPLETE FLOW:
    ///   1. DALL-E gives you: https://oaidalleapiprodscus.blob.core.windows.net/.../cake.png
    ///   2. Call import_image_from_url → Get back ID: 5
    ///   3. Call create_ccontent with Markdown. CHATGPT: For DALL-E images: 1) Generate with DALL-E→get URL, 2) import_image_from_url(that URL)→get ID:5, 3) In content write: ![Cake](https://bitsbybeier.de/api/images/5), 4) Set imageIds='5'. DALL-E URLs fully supported in import_image_from_url!
    ///   4. Done! Image is stored permanently and displayed in the article
    /// </summary>
    /// <param name="author">Author name (required, max 200 characters).</param>
    /// <param name="title">Title of the content (required, max 500 characters).</param>
    /// <param name="content">Main content text with Markdown support (required).</param>
    /// <param name="subtitle">Optional subtitle or summary (max 1000 characters).</param>
    /// <param name="draft">Whether to create as draft (default: true).</param>
    /// <param name="imageIds">Optional comma-separated image IDs to attach (e.g., "1,2,3").</param>
    /// <returns>A message indicating the content was created successfully with details.</returns>
    [McpServerTool(Name = "create_content")]
    [Description("Creates a new content item with Markdown support. To add AI-generated images: 1) Generate image and get URL, 2) Import with import_image_from_url (get ID), 3) Insert in content text using ![alt](https://bitsbybeier.de/api/images/{ID}), 4) List all image IDs in imageIds parameter (comma-separated).")]
    public async Task<string> CreateContentAsync(
        [Description("Author name")] string author,
        [Description("Content title")] string title,
        [Description("Main content text (Markdown supported). To insert images use: ![alt text](https://bitsbybeier.de/api/images/{imageId})")] string content,
        [Description("Optional subtitle")] string? subtitle = null,
        [Description("Create as draft (default: true)")] bool draft = true,
        [Description("Comma-separated image IDs (e.g., '1,2,3')")] string? imageIds = null)
    {
        _logger.LogInformation("=== create_content called via MCP ===");
        _logger.LogInformation("Parameters - Author: {Author}, Title: {Title}, Draft: {Draft}, ImageIds: {ImageIds}", 
            author, title, draft, imageIds);
        
        try
        {
            // Parse comma-separated image IDs
            List<int>? imageIdList = null;
            if (!string.IsNullOrWhiteSpace(imageIds))
            {
                imageIdList = imageIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.Parse(id.Trim()))
                    .ToList();
            }
            
            var request = new ContentRequest
            {
                Author = author,
                Title = title,
                Content = content,
                Subtitle = subtitle,
                Draft = draft,
                ImageIds = imageIdList
            };

            _logger.LogInformation("Creating content via MCP: {Title}, Draft: {Draft}, Images: {ImageCount}", 
                title, draft, imageIdList?.Count ?? 0);

            var createdContent = await _contentService.CreateContentAsync(request);

            var imageInfo = imageIdList?.Count > 0 
                ? $", Attached Images: {imageIdList.Count}" 
                : "";
                
            var result = $"Content created successfully. ID: {createdContent.Id}, Title: {createdContent.Title}, Draft: {createdContent.Draft}, Created: {createdContent.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC{imageInfo}";
            _logger.LogInformation("Content created via MCP with ID: {ContentId}", createdContent.Id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating content via MCP");
            throw new InvalidOperationException($"Error creating content: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Uploads an image from Base64-encoded data. The image can then be attached to content items.
    /// NOTE: For AI-generated images, use import_image_from_url instead to avoid Base64 encoding overhead.
    /// This tool is intended for manual CMS uploads of small images.
    /// Supported formats: JPEG, PNG, GIF, WebP, SVG. Maximum size: 10MB (effective ~7.5MB due to Base64 overhead).
    /// Security: Images are validated by checking file signatures (magic bytes) to ensure they match the specified type.
    /// </summary>
    /// <param name="base64Data">Base64-encoded image data (without data URI prefix).</param>
    /// <param name="fileName">Original filename (e.g., "photo.jpg").</param>
    /// <param name="contentType">MIME type (e.g., "image/jpeg", "image/png").</param>
    /// <returns>A message with the image ID that can be used to attach to content.</returns>
    [McpServerTool(Name = "upload_image")]
    [Description("Uploads an image from Base64 data. DEPRECATED for AI use - use import_image_from_url instead for better performance. This tool is for manual CMS uploads only. Max effective size: ~7.5MB.")]
    public async Task<string> UploadImageAsync(
        [Description("Base64-encoded image data")] string base64Data,
        [Description("Filename with extension (e.g., 'image.jpg')")] string fileName,
        [Description("MIME type (image/jpeg, image/png, image/gif, image/webp, image/svg+xml)")] string contentType)
    {
        _logger.LogInformation("=== upload_image called via MCP ===");
        _logger.LogInformation("Parameters - FileName: {FileName}, ContentType: {ContentType}", fileName, contentType);
        
        try
        {
            _logger.LogInformation("Uploading image via MCP: {FileName}, Type: {ContentType}", fileName, contentType);

            var image = await _imageService.UploadImageAsync(base64Data, fileName, contentType);

            var result = $"Image uploaded successfully. ID: {image.Id}, Filename: {image.FileName}, Size: {image.FileSize} bytes, Type: {image.ContentType}. Use this ID when creating content to attach this image.";
            _logger.LogInformation("Image uploaded via MCP with ID: {ImageId}", image.Id);
            
            return result;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error uploading image via MCP");
            throw new InvalidOperationException($"Error uploading image: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading image via MCP");
            throw new InvalidOperationException($"Unexpected error uploading image: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Imports an image from ANY public URL by downloading it to the server.
    /// ✅ FULLY SUPPORTS DALL-E TEMPORARY URLS (oaidalleapiprodscus.blob.core.windows.net)
    /// ✅ NO CORS RESTRICTIONS - Server downloads on your behalf
    /// ✅ NO URL ALLOWLIST - All public HTTP/HTTPS URLs accepted
    /// ✅ AUTO OPTIMIZATION - Resizes if needed, compresses for web
    /// 
    /// CHATGPT WORKFLOW:
    /// 1. Generate image with DALL-E → You receive temporary URL like:
    ///    "https://oaidalleapiprodscus.blob.core.windows.net/private/org-xyz/user-abc/img-123.png?..."
    /// 2. Call THIS TOOL with that exact URL → Returns: "Image imported successfully. ID: 5"
    /// 3. Use the ID in create_content: In content parameter write "![Cake](https://bitsbybeier.de/api/images/5)"
    /// 4. Add ID to imageIds parameter: "5" (or "5,7,9" for multiple images)
    /// 
    /// TECHNICAL DETAILS:
    /// - Server downloads from ANY URL (bypasses CORS completely)
    /// - Auto-resizes if > 1920x1920px (maintains aspect ratio)
    /// - JPEG quality: 85%, PNG: compressed
    /// - Max original size: 10MB (usually 2-5MB after optimization)
    /// - Supported formats: JPEG, PNG, GIF, WebP, SVG
    /// - No external service dependencies
    /// 
    /// DALL-E SPECIFIC:
    /// - 1024x1024 standard images: ~500KB-1MB (optimal)
    /// - 1792x1024 HD images: ~1-2MB (optimal)
    /// - All DALL-E URLs work - temporary blob storage URLs are fully supported
    /// </summary>
    /// <param name="imageUrl">ANY public image URL. DALL-E temporary URLs (oaidalleapiprodscus.blob.core.windows.net) are fully supported. Example: https://oaidalleapiprodscus.blob.core.windows.net/private/org-xyz/user-abc/img-123.png?se=2026...</param>
    /// <param name="description">Optional description for alt text (e.g., 'A chocolate cake with frosting'). Used in Markdown syntax.</param>
    /// <param name="fileName">Optional custom filename (e.g., 'chocolate-cake.jpg'). Auto-generated if not provided.</param>
    /// <returns>A message with the image ID that can be used when creating content.</returns>
    [McpServerTool(Name = "import_image_from_url")]
    [Description("✅ Downloads image from ANY URL including DALL-E temporary URLs. NO CORS/allowlist restrictions - server downloads for you. Returns image ID to use in content. CHATGPT: Use for all DALL-E images! Step 1: Generate image, Step 2: Call this with URL, Step 3: Use returned ID in create_content.")]
    public async Task<string> ImportImageFromUrlAsync(
        [Description("Public URL of the image to download (e.g., https://oaidalleapiprodscus.blob.core.windows.net/...)")] string imageUrl,
        [Description("Optional description for alt text (e.g., 'A chocolate cake with frosting')")] string? description = null,
        [Description("Optional custom filename (e.g., 'chocolate-cake.jpg'). Auto-generated if not provided.")] string? fileName = null)
    {
        _logger.LogInformation("=== import_image_from_url called via MCP ===");
        _logger.LogInformation("Parameters - ImageUrl: {ImageUrl}, FileName: {FileName}", imageUrl, fileName);
        
        try
        {
            _logger.LogInformation("Importing image from URL via MCP: {ImageUrl}", imageUrl);

            var image = await _imageService.ImportImageFromUrlAsync(imageUrl, fileName);

            var result = $"Image imported successfully. ID: {image.Id}, Filename: {image.FileName}, Size: {image.FileSize} bytes, Type: {image.ContentType}. Use this ID when creating content to attach this image. In your content text, reference it as: ![{description ?? "image"}](https://bitsbybeier.de/api/images/{image.Id})";
            _logger.LogInformation("Image imported from URL via MCP with ID: {ImageId}", image.Id);
            
            return result;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error importing image from URL via MCP");
            throw new InvalidOperationException($"Error importing image: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error importing image from URL via MCP");
            throw new InvalidOperationException($"Unexpected error importing image: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Generates an image using OpenAI's DALL-E API and automatically saves it to the server.
    /// This is the RECOMMENDED way to add AI-generated images to content.
    /// 
    /// NEW WORKFLOW FOR AI-GENERATED CONTENT WITH IMAGES:
    /// 1. LLM receives request to create article with images
    /// 2. LLM calls THIS TOOL (generate_image) with descriptive prompts → Returns image IDs
    /// 3. LLM creates article content with Markdown syntax using those IDs: ![alt](https://bitsbybeier.de/api/images/{ID})
    /// 4. LLM calls create_content with the markdown content and imageIds parameter
    /// 
    /// EXAMPLE:
    /// Step 1: Call generate_image(prompt="A delicious chocolate cake with frosting") → Returns "Image generated successfully. ID: 5"
    /// Step 2: Call generate_image(prompt="A bakery storefront") → Returns "Image generated successfully. ID: 6"
    /// Step 3: Create article with content: "# Bakery Guide\n\n![Chocolate Cake](https://bitsbybeier.de/api/images/5)\n\n![Our Store](https://bitsbybeier.de/api/images/6)"
    /// Step 4: Call create_content with imageIds="5,6" to attach images
    /// 
    /// BENEFITS:
    /// - No need to manually import from DALL-E URLs
    /// - Images are automatically saved and optimized
    /// - Simpler workflow - one tool call per image
    /// - Consistent naming and organization
    /// 
    /// CONFIGURATION:
    /// Requires OPENAI_API_KEY environment variable to be set.
    /// Default model: dall-e-3, size: 1024x1024, quality: standard.
    /// </summary>
    /// <param name="prompt">Detailed description of the image to generate (be specific for best results).</param>
    /// <param name="size">Optional image size: "1024x1024" (square), "1792x1024" (landscape), or "1024x1792" (portrait). Default: 1024x1024.</param>
    /// <param name="quality">Optional quality: "standard" (faster, cheaper) or "hd" (higher detail). Default: standard.</param>
    /// <param name="model">Optional model: "dall-e-3" (better quality) or "dall-e-2" (faster). Default: dall-e-3.</param>
    /// <returns>A message with the image ID that can be used when creating content.</returns>
    [McpServerTool(Name = "generate_image")]
    [Description("✅ RECOMMENDED: Generates an image using OpenAI DALL-E and auto-saves to server. Returns image ID for use in content. CHATGPT: Use this for all image generation! Step 1: Call this tool with prompt, Step 2: Get back image ID, Step 3: Use ID in create_content markdown.")]
    public async Task<string> GenerateImageAsync(
        [Description("Detailed text description of the image to generate (e.g., 'A modern minimalist bakery interior with wooden tables')")] string prompt,
        [Description("Optional size: '1024x1024' (square), '1792x1024' (landscape), or '1024x1792' (portrait)")] string? size = null,
        [Description("Optional quality: 'standard' (default) or 'hd'")] string? quality = null,
        [Description("Optional model: 'dall-e-3' (default) or 'dall-e-2'")] string? model = null)
    {
        _logger.LogInformation("=== generate_image called via MCP ===");
        _logger.LogInformation("Parameters - Prompt: {Prompt}, Size: {Size}, Quality: {Quality}, Model: {Model}", 
            prompt, size, quality, model);
        
        try
        {
            _logger.LogInformation("Generating image via OpenAI DALL-E: {Prompt}", prompt);

            var image = await _openAIImageService.GenerateImageAsync(prompt, size, quality, model);

            var result = $"Image generated successfully. ID: {image.Id}, Filename: {image.FileName}, Size: {image.FileSize} bytes, Type: {image.ContentType}. Use this ID when creating content to attach this image. In your content text, reference it as: ![Generated image](https://bitsbybeier.de/api/images/{image.Id})";
            _logger.LogInformation("Image generated and saved via MCP with ID: {ImageId}", image.Id);
            
            return result;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error generating image via MCP");
            throw new InvalidOperationException($"Error generating image: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating image via MCP");
            throw new InvalidOperationException($"Unexpected error generating image: {ex.Message}", ex);
        }
    }
}
