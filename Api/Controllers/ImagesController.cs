using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using bitsbybeier.Api.Services;
using bitsbybeier.Data;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Controller for image upload and management operations.
/// </summary>
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ImagesController : BaseController
{
    private readonly IImageService _imageService;

    /// <summary>
    /// Initializes a new instance of the ImagesController.
    /// </summary>
    public ImagesController(
        ILogger<ImagesController> logger,
        ApplicationDbContext context,
        IImageService imageService)
        : base(logger, context)
    {
        _imageService = imageService;
    }

    /// <summary>
    /// Uploads a new image from Base64-encoded data.
    /// </summary>
    /// <param name="request">Image upload request with Base64 data.</param>
    /// <returns>The uploaded image metadata.</returns>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage([FromBody] ImageUploadRequest request)
    {
        try
        {
            var image = await _imageService.UploadImageAsync(
                request.Base64Data, 
                request.FileName, 
                request.ContentType,
                request.ContentId);

            var response = new ImageResponse
            {
                Id = image.Id,
                FileName = image.FileName,
                ContentType = image.ContentType,
                FileSize = image.FileSize,
                UploadedAt = image.UploadedAt,
                ContentId = image.ContentId
            };

            return Created($"/api/images/{image.Id}", response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves an image by ID.
    /// </summary>
    /// <param name="id">Image ID.</param>
    /// <param name="maxWidth">Optional max width for resizing (default: no resize).</param>
    /// <param name="maxHeight">Optional max height for resizing (default: no resize).</param>
    /// <returns>The image file.</returns>
    [HttpGet("{id}")]
    [AllowAnonymous] // Allow public access to view images
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImage(int id, [FromQuery] int? maxWidth = null, [FromQuery] int? maxHeight = null)
    {
        var image = await _imageService.GetImageAsync(id);
        if (image == null)
        {
            return NotFound();
        }
        
        // If resize parameters provided, resize the image
        if ((maxWidth.HasValue || maxHeight.HasValue) && image.ContentType != "image/svg+xml")
        {
            var resizedData = await _imageService.ResizeImageAsync(
                image.ImageData, 
                image.ContentType, 
                maxWidth ?? 1920, 
                maxHeight ?? 1920
            );
            return File(resizedData, image.ContentType);
        }

        // Return file inline (no download) by using only 2 parameters
        return File(image.ImageData, image.ContentType);
    }

    /// <summary>
    /// Gets metadata for an image.
    /// </summary>
    /// <param name="id">Image ID.</param>
    /// <returns>Image metadata.</returns>
    [HttpGet("{id}/metadata")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImageMetadata(int id)
    {
        var image = await _imageService.GetImageAsync(id);
        if (image == null)
        {
            return NotFound();
        }

        var response = new ImageResponse
        {
            Id = image.Id,
            FileName = image.FileName,
            ContentType = image.ContentType,
            FileSize = image.FileSize,
            UploadedAt = image.UploadedAt,
            ContentId = image.ContentId > 0 ? image.ContentId : null
        };

        return Ok(response);
    }

    /// <summary>
    /// Attaches an existing image to content.
    /// </summary>
    /// <param name="id">Image ID.</param>
    /// <param name="request">Request with content ID.</param>
    /// <returns>Success message.</returns>
    [HttpPost("{id}/attach")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AttachToContent(int id, [FromBody] AttachImageRequest request)
    {
        try
        {
            await _imageService.AttachImageToContentAsync(id, request.ContentId);
            return Ok(new { message = $"Image {id} attached to content {request.ContentId}" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all images for a content item.
    /// </summary>
    /// <param name="contentId">Content ID.</param>
    /// <returns>List of images.</returns>
    [HttpGet("content/{contentId}")]
    [AllowAnonymous] // Allow public access for viewing content images
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContentImages(int contentId)
    {
        var images = await _imageService.GetContentImagesAsync(contentId);

        var response = images.Select(i => new ImageResponse
        {
            Id = i.Id,
            FileName = i.FileName,
            ContentType = i.ContentType,
            FileSize = i.FileSize,
            UploadedAt = i.UploadedAt,
            ContentId = i.ContentId
        });

        return Ok(response);
    }
}

/// <summary>
/// Request model for uploading images.
/// </summary>
public record ImageUploadRequest
{
    /// <summary>
    /// Base64-encoded image data.
    /// </summary>
    public required string Base64Data { get; init; }

    /// <summary>
    /// Original filename.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// MIME content type (e.g., "image/jpeg").
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Optional content ID to attach to immediately.
    /// </summary>
    public int? ContentId { get; init; }
}

/// <summary>
/// Response model for image metadata.
/// </summary>
public record ImageResponse
{
    public required int Id { get; init; }
    public string? FileName { get; init; }
    public required string ContentType { get; init; }
    public required long FileSize { get; init; }
    public required DateTime UploadedAt { get; init; }
    public int? ContentId { get; init; }
}

/// <summary>
/// Request model for attaching images to content.
/// </summary>
public record AttachImageRequest
{
    public required int ContentId { get; init; }
}
