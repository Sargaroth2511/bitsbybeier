using Microsoft.EntityFrameworkCore;
using bitsbybeier.Data;
using bitsbybeier.Domain.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Webp;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for managing content images with validation and security checks.
/// </summary>
public class ImageService : IImageService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImageService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    
    // Allowed image MIME types
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml"
    };
    
    // Maximum file size: 10MB
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    
    // Maximum image dimensions for web display
    private const int MaxImageWidth = 1920;
    private const int MaxImageHeight = 1920;
    
    public ImageService(ApplicationDbContext context, ILogger<ImageService> logger, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }
    
    /// <inheritdoc/>
    public bool IsImageTypeAllowed(string contentType)
    {
        return AllowedImageTypes.Contains(contentType);
    }
    
    /// <inheritdoc/>
    public async Task<ContentImage> UploadImageAsync(string base64Data, string fileName, string contentType, int? contentId = null)
    {
        _logger.LogInformation("Uploading image: {FileName}, ContentType: {ContentType}", fileName, contentType);
        
        // Validate content type
        if (!IsImageTypeAllowed(contentType))
        {
            throw new InvalidOperationException($"Image type '{contentType}' is not allowed. Allowed types: {string.Join(", ", AllowedImageTypes)}");
        }
        
        // Decode Base64
        byte[] imageData;
        try
        {
            imageData = Convert.FromBase64String(base64Data);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Invalid Base64 image data", ex);
        }
        
        // Validate file size
        if (imageData.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"Image size ({imageData.Length} bytes) exceeds maximum allowed size ({MaxFileSizeBytes} bytes)");
        }
        
        // Validate it's actually an image by checking magic bytes
        if (!IsValidImageData(imageData, contentType))
        {
            throw new InvalidOperationException("File data does not match the specified image type");
        }
        
        // If contentId is provided, verify it exists
        if (contentId.HasValue)
        {
            var contentExists = await _context.Contents.AnyAsync(c => c.Id == contentId.Value);
            if (!contentExists)
            {
                throw new InvalidOperationException($"Content with ID {contentId.Value} not found");
            }
        }
        
        var image = new ContentImage
        {
            ImageData = imageData,
            ContentType = contentType,
            FileName = fileName,
            FileSize = imageData.Length,
            UploadedAt = DateTime.UtcNow,
            ContentId = contentId // Null if not attached to content yet
        };
        
        _context.ContentImages.Add(image);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Image uploaded successfully with ID: {ImageId}", image.Id);
        
        return image;
    }
    
    /// <inheritdoc/>
    public async Task<ContentImage> ImportImageFromUrlAsync(string imageUrl, string? fileName = null, int? contentId = null)
    {
        _logger.LogInformation("Importing image from URL: {ImageUrl}", imageUrl);
        
        // Validate URL format
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) || 
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Invalid image URL. Must be a valid HTTP or HTTPS URL.");
        }
        
        // Download image
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        byte[] imageData;
        string contentType;
        
        try
        {
            using var response = await httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();
            
            // Get content type from response
            contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            
            // Validate content type
            if (!IsImageTypeAllowed(contentType))
            {
                throw new InvalidOperationException($"URL content type '{contentType}' is not an allowed image type. Allowed types: {string.Join(", ", AllowedImageTypes)}");
            }
            
            // Download image data
            imageData = await response.Content.ReadAsByteArrayAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to download image from URL: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new InvalidOperationException("Image download timed out after 30 seconds", ex);
        }
        
        // Validate file size
        if (imageData.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"Downloaded image size ({imageData.Length} bytes) exceeds maximum allowed size ({MaxFileSizeBytes} bytes)");
        }
        
        // Validate image data by checking magic bytes
        if (!IsValidImageData(imageData, contentType))
        {
            throw new InvalidOperationException("Downloaded file data does not match the expected image type");
        }
        
        // Resize image if it's too large (skip for SVG as it's vector-based)
        if (contentType != "image/svg+xml")
        {
            imageData = await ResizeImageIfNeededAsync(imageData, contentType);
        }
        
        // Extract filename from URL if not provided
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = Path.GetFileName(uri.LocalPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"image-{DateTime.UtcNow:yyyyMMddHHmmss}{GetFileExtension(contentType)}";
            }
        }
        
        // If contentId is provided, verify it exists
        if (contentId.HasValue)
        {
            var contentExists = await _context.Contents.AnyAsync(c => c.Id == contentId.Value);
            if (!contentExists)
            {
                throw new InvalidOperationException($"Content with ID {contentId.Value} not found");
            }
        }
        
        var image = new ContentImage
        {
            ImageData = imageData,
            ContentType = contentType,
            FileName = fileName,
            FileSize = imageData.Length,
            UploadedAt = DateTime.UtcNow,
            ContentId = contentId
        };
        
        _context.ContentImages.Add(image);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Image imported successfully from URL with ID: {ImageId}, Size: {FileSize} bytes", image.Id, image.FileSize);
        
        return image;
    }
    
    /// <inheritdoc/>
    public async Task<ContentImage?> GetImageAsync(int imageId)
    {
        return await _context.ContentImages.FindAsync(imageId);
    }
    
    /// <inheritdoc/>
    public async Task AttachImageToContentAsync(int imageId, int contentId)
    {
        _logger.LogInformation("Attaching image {ImageId} to content {ContentId}", imageId, contentId);
        
        var image = await _context.ContentImages.FindAsync(imageId);
        if (image == null)
        {
            throw new InvalidOperationException($"Image with ID {imageId} not found");
        }
        
        var content = await _context.Contents.FindAsync(contentId);
        if (content == null)
        {
            throw new InvalidOperationException($"Content with ID {contentId} not found");
        }
        
        image.ContentId = contentId;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Image attached successfully");
    }
    
    /// <inheritdoc/>
    public async Task<List<ContentImage>> GetContentImagesAsync(int contentId)
    {
        return await _context.ContentImages
            .Where(i => i.ContentId == contentId)
            .OrderBy(i => i.UploadedAt)
            .ToListAsync();
    }
    
    /// <summary>
    /// Resizes an image if it exceeds maximum dimensions while maintaining aspect ratio.
    /// </summary>
    private async Task<byte[]> ResizeImageIfNeededAsync(byte[] imageData, string contentType)
    {
        using var inputStream = new MemoryStream(imageData);
        using var image = await Image.LoadAsync(inputStream);
        
        // Check if resizing is needed
        if (image.Width <= MaxImageWidth && image.Height <= MaxImageHeight)
        {
            _logger.LogInformation("Image dimensions ({Width}x{Height}) are within limits, no resize needed", image.Width, image.Height);
            return imageData; // No resize needed
        }
        
        _logger.LogInformation("Resizing image from {OriginalWidth}x{OriginalHeight} to max {MaxWidth}x{MaxHeight}", 
            image.Width, image.Height, MaxImageWidth, MaxImageHeight);
        
        return await ResizeImageAsync(imageData, contentType, MaxImageWidth, MaxImageHeight);
    }
    
    /// <inheritdoc/>
    public async Task<byte[]> ResizeImageAsync(byte[] imageData, string contentType, int maxWidth, int maxHeight)
    {
        using var inputStream = new MemoryStream(imageData);
        using var image = await Image.LoadAsync(inputStream);
        
        var originalSize = imageData.Length;
        var originalDimensions = $"{image.Width}x{image.Height}";
        
        // Calculate new dimensions maintaining aspect ratio
        var ratioX = (double)maxWidth / image.Width;
        var ratioY = (double)maxHeight / image.Height;
        var ratio = Math.Min(ratioX, ratioY);
        
        // If ratio >= 1, no resize needed (image is smaller than max dimensions)
        if (ratio >= 1.0)
        {
            _logger.LogInformation("Image dimensions ({Width}x{Height}) are within limits {MaxWidth}x{MaxHeight}, no resize needed", 
                image.Width, image.Height, maxWidth, maxHeight);
            return imageData;
        }
        
        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);
        
        _logger.LogInformation("Resizing image from {OriginalDimensions} to {NewWidth}x{NewHeight}", originalDimensions, newWidth, newHeight);
        
        // Resize the image
        image.Mutate(x => x.Resize(newWidth, newHeight));
        
        // Save to memory stream with appropriate encoder
        using var outputStream = new MemoryStream();
        
        switch (contentType.ToLowerInvariant())
        {
            case "image/png":
                await image.SaveAsPngAsync(outputStream, new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression });
                break;
            case "image/jpeg":
            case "image/jpg":
                await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 85 });
                break;
            case "image/gif":
                await image.SaveAsGifAsync(outputStream);
                break;
            case "image/webp":
                await image.SaveAsWebpAsync(outputStream, new WebpEncoder { Quality = 85 });
                break;
            default:
                await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 85 });
                break;
        }
        
        var resizedData = outputStream.ToArray();
        var newSize = resizedData.Length;
        var savedBytes = originalSize - newSize;
        var savedPercent = (savedBytes * 100.0) / originalSize;
        
        _logger.LogInformation("Image resized successfully. Original: {OriginalSize} bytes, New: {NewSize} bytes, Saved: {SavedBytes} bytes ({SavedPercent:F1}%)", 
            originalSize, newSize, savedBytes, savedPercent);
        
        return resizedData;
    }
    
    /// <summary>
    /// Gets file extension from content type.
    /// </summary>
    private string GetFileExtension(string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            _ => ".jpg"
        };
    }
    
    /// <summary>
    /// Validates image data by checking magic bytes (file signatures).
    /// </summary>
    private bool IsValidImageData(byte[] data, string contentType)
    {
        if (data.Length < 4)
        {
            return false;
        }
        
        // Check magic bytes based on content type
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
            "image/png" => data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47,
            "image/gif" => data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46,
            "image/webp" => data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46,
            "image/svg+xml" => true, // SVG is XML, harder to validate by bytes
            _ => false
        };
    }
}
