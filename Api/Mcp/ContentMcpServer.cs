using ModelContextProtocol.Server;
using bitsbybeier.Api.Models;
using bitsbybeier.Api.Services;
using System.ComponentModel;

namespace bitsbybeier.Api.Mcp;

/// <summary>
/// MCP Server for content management operations.
/// Provides AI agents with the ability to create content items and manage images programmatically.
/// 
/// ✅ PRIMARY WORKFLOW - Use generate_image for ALL Image Generation:
/// 1. Call generate_image(prompt="A chocolate cake with frosting") → Returns: "Image generated successfully. ID: 5"
/// 2. Repeat for each image needed: generate_image(prompt="A bakery") → Returns: "ID: 6"
/// 3. Create content with Markdown image syntax: ![Alt Text](https://bitsbybeier.de/api/images/5)
/// 4. Call create_content with imageIds="5,6" to attach all images
/// 
/// COMPLETE EXAMPLE:
/// User: "Create article about chocolate cake with images"
/// You: Call generate_image(prompt="A delicious chocolate cake with chocolate frosting") → Get ID: 5
/// You: Write content: "# Chocolate Cake\n\n![Chocolate Cake](https://bitsbybeier.de/api/images/5)\n\nThis cake..."
/// You: Call create_content(title="Chocolate Cake", content=above, imageIds="5")
/// 
/// IMPORTANT RULES:
/// - Always use generate_image for image generation (NOT external DALL-E)
/// - Image URLs MUST be: https://bitsbybeier.de/api/images/{ID}
/// - Include newlines as \n in content parameter
/// - List all image IDs in imageIds parameter (comma-separated)
/// </summary>
[McpServerToolType]
public class ContentMcpTools
{
    private readonly IContentService _contentService;
    private readonly IImageService _imageService;
    private readonly IOpenAIImageService _openAIImageService;
    private readonly ILogger<ContentMcpTools> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the ContentMcpTools.
    /// </summary>
    /// <param name="contentService">Content service for content operations.</param>
    /// <param name="imageService">Image service for image operations.</param>
    /// <param name="openAIImageService">OpenAI image service for AI image generation.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="configuration">Configuration for reading base URL.</param>
    public ContentMcpTools(
        IContentService contentService, 
        IImageService imageService,
        IOpenAIImageService openAIImageService,
        ILogger<ContentMcpTools> logger,
        IConfiguration configuration)
    {
        _contentService = contentService;
        _imageService = imageService;
        _openAIImageService = openAIImageService;
        _logger = logger;
        _configuration = configuration;
        
        _logger.LogInformation("=== ContentMcpTools instance created ===");
        _logger.LogInformation("Tools should be available:");
        _logger.LogInformation("  - create_content (CreateContentAsync)");
        _logger.LogInformation("  - upload_image (UploadImageAsync)");
        _logger.LogInformation("  - import_image_from_url (ImportImageFromUrlAsync)");
        _logger.LogInformation("  - generate_image (GenerateImageAsync)");
    }
    
    private string GetBaseUrl()
    {
        // In development, use relative URL for Angular proxy compatibility
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        if (environment == "Development")
        {
            return ""; // Empty string = relative URL like /api/images/1
        }
        return "https://bitsbybeier.de";
    }

    /// <summary>
    /// Creates a new content item in the CMS. Content is created as draft by default (not published).
    /// Use this to add articles, blog posts, or any text-based content.
    /// Content supports Markdown formatting, including links and references.
    /// 
    /// ⚠️ CHATGPT: IF USER REQUESTS IMAGES WITH THE ARTICLE:
    /// YOU MUST CALL generate_image TOOL FIRST! DO NOT write placeholder text like [IMAGE_1: ...]!
    /// 
    /// COMPLETE WORKFLOW WITH IMAGES:
    /// 
    /// Example: User says "Create article about Viking god Tyr with 3 images"
    /// 
    /// STEP 1: Generate each image using generate_image tool
    ///   - Call: generate_image(prompt="Stylized drawing of Viking god Tyr, one-armed, with spear and shield, runes in background, muted colors")
    ///   - Response: "Image generated successfully. ID: 42"
    ///   - Repeat for each image requested (call generate_image multiple times!)
    ///   - Example results: ID: 42, ID: 43, ID: 44
    /// 
    /// STEP 2: Write article content including the images in Markdown format
    ///   - Format: ![Description](https://bitsbybeier.de/api/images/{ID})
    ///   - Example: "# Tyr\n\n![Tyr the God](https://bitsbybeier.de/api/images/42)\n\nTyr was a Norse god..."
    ///   - Include all generated images in your content
    /// 
    /// STEP 3: Create content with imageIds parameter
    ///   - Call create_content with imageIds="42,43,44" (comma-separated list of all IDs)
    ///   - Content parameter contains the full article with Markdown image references
    ///   - Done! Article with images is created
    /// 
    /// CRITICAL: Always use generate_image tool when images are requested. Never create placeholder text!
    /// </summary>
    /// <param name="author">Author name (required, max 200 characters).</param>
    /// <param name="title">Title of the content (required, max 500 characters).</param>
    /// <param name="content">Main content text with Markdown support (required).</param>
    /// <param name="subtitle">Optional subtitle or summary (max 1000 characters).</param>
    /// <param name="draft">Whether to create as draft (default: true).</param>
    /// <param name="imageIds">Optional comma-separated image IDs to attach (e.g., "1,2,3").</param>
    /// <returns>A message indicating the content was created successfully with details.</returns>
    [McpServerTool(Name = "create_content")]
    [Description("Creates blog article/content. ⚠️ For images: ALWAYS call generate_image FIRST (once per image)! Get IDs, then include as ![Alt](https://bitsbybeier.de/api/images/{ID}), set imageIds='5,6,7'. DO NOT write [IMAGE:...] placeholders - actually generate them with generate_image tool!")]
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
            
            var baseUrl = GetBaseUrl();
            var viewUrl = draft 
                ? $"{baseUrl}/drafts" 
                : $"{baseUrl}/content";
                
            var result = $"✅ Article created successfully!\n\nID: {createdContent.Id}\nTitle: {createdContent.Title}\nStatus: {(draft ? "Draft" : "Published")}\nCreated: {createdContent.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC{imageInfo}\n\nThe article is ready and can be viewed at: {viewUrl}\n\nNext steps: {(draft ? "The article is saved as a draft. You can review it, edit if needed, and publish it from the CMS." : "The article is now live and publicly visible.")}";            _logger.LogInformation("Content created via MCP with ID: {ContentId}", createdContent.Id);
            
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

            var baseUrl = GetBaseUrl();
            var result = $"Image imported successfully. ID: {image.Id}, Filename: {image.FileName}, Size: {image.FileSize} bytes, Type: {image.ContentType}. Use this ID when creating content to attach this image. In your content text, reference it as: ![{description ?? "image"}]({baseUrl}/api/images/{image.Id})";
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
    /// ✅ THIS IS THE PRIMARY TOOL FOR IMAGE GENERATION - Use this instead of external DALL-E!
    /// 
    /// CHATGPT: ALWAYS USE THIS TOOL FOR IMAGE GENERATION
    /// DO NOT generate images externally - this tool handles everything automatically.
    /// 
    /// SIMPLE 2-STEP WORKFLOW:
    /// Step 1: Call generate_image(prompt="A chocolate cake with frosting")
    ///         Returns: "Image generated successfully. ID: 5"
    /// 
    /// Step 2: Use ID in create_content markdown:
    ///         content="# Cake Recipe\n\n![Chocolate Cake](https://bitsbybeier.de/api/images/5)\n\nDelicious!"
    ///         imageIds="5"
    /// 
    /// MULTIPLE IMAGES EXAMPLE:
    /// generate_image("chocolate cake") → ID: 5
    /// generate_image("bakery interior") → ID: 6
    /// create_content with content="![Cake](https://bitsbybeier.de/api/images/5)\n\n![Store](https://bitsbybeier.de/api/images/6)" and imageIds="5,6"
    /// 
    /// IMAGE URL FORMAT (REQUIRED):
    /// - Production: https://bitsbybeier.de/api/images/{ID}
    /// - Always use the ID returned by this tool
    /// 
    /// WHAT HAPPENS AUTOMATICALLY:
    /// - Calls OpenAI DALL-E API
    /// - Downloads generated image
    /// - Saves to database with optimization
    /// - Returns image ID for immediate use
    /// 
    /// DEFAULT SETTINGS:
    /// - Model: dall-e-3 (best quality)
    /// - Size: 1024x1024 (square)
    /// - Quality: standard (fast and good)
    /// </summary>
    /// <param name="prompt">Detailed description of the image to generate (be specific for best results).</param>
    /// <param name="size">Optional image size: "1024x1024" (square), "1792x1024" (landscape), or "1024x1792" (portrait). Default: 1024x1024.</param>
    /// <param name="quality">Optional quality: "standard" (faster, cheaper) or "hd" (higher detail). Default: standard.</param>
    /// <param name="model">Optional model: "dall-e-3" (better quality) or "dall-e-2" (faster). Default: dall-e-3.</param>
    /// <returns>A message with the image ID that can be used when creating content.</returns>
    [McpServerTool(Name = "generate_image")]
    [Description("🎨 AI IMAGE GENERATOR: Creates images using AI (OpenAI DALL-E or Stability AI). Saves automatically, returns ID. When user requests images, call this tool MULTIPLE TIMES (once per image). DO NOT write placeholders! Example workflow: User wants 3 images→Call generate_image 3x→Get IDs 5,6,7→Include in article: ![Image](https://bitsbybeier.de/api/images/5). Prompt examples: 'A Viking god Tyr with one arm, stylized drawing, muted colors'")]
    public async Task<string> GenerateImageAsync(
        [Description("Detailed text description of the image to generate (e.g., 'A modern minimalist bakery interior with wooden tables')")] string prompt,
        [Description("Size/aspect ratio. OpenAI: '1024x1024' (square), '1792x1024' (landscape), '1024x1792' (portrait). Stability: '1:1', '16:9', '9:16', '21:9', '2:3', '3:2', '4:5', '5:4'")] string? size = null,
        [Description("Quality: 'standard' (default, fast) or 'hd' (OpenAI only, higher detail). For Stability, use 'photographic', 'cinematic', 'anime', '3d-model', etc.")] string? quality = null,
        [Description("Model: 'dall-e-3' (best), 'dall-e-2' (OpenAI) OR 'core' (fast), 'ultra' (photorealistic), 'sd3.5-large', 'sd3.5-medium', 'sd3.5-large-turbo' (Stability AI). Provider auto-selected based on model.")] string? model = null)
    {
        _logger.LogInformation("=== generate_image called via MCP ===");
        _logger.LogInformation("Parameters - Prompt: {Prompt}, Size: {Size}, Quality: {Quality}, Model: {Model}", 
            prompt, size, quality, model);
        
        try
        {
            _logger.LogInformation("Generating image via OpenAI DALL-E: {Prompt}", prompt);

            var image = await _openAIImageService.GenerateImageAsync(prompt, size, quality, model);

            var baseUrl = GetBaseUrl();
            var result = $"✅ Image generated and saved successfully!\n\nImage ID: {image.Id}\nFilename: {image.FileName}\nSize: {image.FileSize} bytes\n\nNext step: Use this ID when creating the article. Include in the content markdown as:\n![Description]({baseUrl}/api/images/{image.Id})\n\nAnd pass imageIds='{image.Id}' parameter to create_content.";
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
