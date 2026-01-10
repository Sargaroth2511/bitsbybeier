using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for managing content images.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Uploads an image from a Base64-encoded string.
    /// For manual CMS uploads only. AI tools should use ImportImageFromUrlAsync.
    /// </summary>
    /// <param name="base64Data">Base64-encoded image data.</param>
    /// <param name="fileName">Original filename.</param>
    /// <param name="contentType">MIME type (e.g., "image/jpeg").</param>
    /// <param name="contentId">Optional content ID to associate with.</param>
    /// <returns>The created image entity.</returns>
    Task<ContentImage> UploadImageAsync(string base64Data, string fileName, string contentType, int? contentId = null);
    
    /// <summary>
    /// Imports an image from a public URL by downloading it.
    /// Recommended for AI-generated images and external sources.
    /// </summary>
    /// <param name="imageUrl">Public URL of the image to download.</param>
    /// <param name="fileName">Optional filename. If null, extracted from URL.</param>
    /// <param name="contentId">Optional content ID to associate with.</param>
    /// <returns>The created image entity.</returns>
    Task<ContentImage> ImportImageFromUrlAsync(string imageUrl, string? fileName = null, int? contentId = null);
    
    /// <summary>
    /// Resizes an image to fit within specified dimensions while maintaining aspect ratio.
    /// </summary>
    /// <param name="imageData">Original image data.</param>
    /// <param name="contentType">MIME type of the image.</param>
    /// <param name="maxWidth">Maximum width.</param>
    /// <param name="maxHeight">Maximum height.</param>
    /// <returns>Resized image data.</returns>
    Task<byte[]> ResizeImageAsync(byte[] imageData, string contentType, int maxWidth, int maxHeight);
    
    /// <summary>
    /// Retrieves an image by ID.
    /// </summary>
    /// <param name="imageId">Image ID.</param>
    /// <returns>The image entity or null if not found.</returns>
    Task<ContentImage?> GetImageAsync(int imageId);
    
    /// <summary>
    /// Attaches an existing image to content.
    /// </summary>
    /// <param name="imageId">Image ID to attach.</param>
    /// <param name="contentId">Content ID to attach to.</param>
    Task AttachImageToContentAsync(int imageId, int contentId);
    
    /// <summary>
    /// Gets all images for a content item.
    /// </summary>
    /// <param name="contentId">Content ID.</param>
    /// <returns>List of images.</returns>
    Task<List<ContentImage>> GetContentImagesAsync(int contentId);
    
    /// <summary>
    /// Validates if an image format is allowed.
    /// </summary>
    /// <param name="contentType">MIME type to validate.</param>
    /// <returns>True if allowed, false otherwise.</returns>
    bool IsImageTypeAllowed(string contentType);
}
