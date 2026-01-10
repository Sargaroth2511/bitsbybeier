using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for generating images using OpenAI's DALL-E API.
/// </summary>
public interface IOpenAIImageService
{
    /// <summary>
    /// Generates an image using OpenAI's DALL-E API and saves it to the server.
    /// </summary>
    /// <param name="prompt">Text prompt describing the image to generate.</param>
    /// <param name="size">Optional image size (e.g., "1024x1024", "1792x1024", "1024x1792"). Uses default if not specified.</param>
    /// <param name="quality">Optional quality setting ("standard" or "hd"). Uses default if not specified.</param>
    /// <param name="model">Optional model to use (e.g., "dall-e-3", "dall-e-2"). Uses default if not specified.</param>
    /// <returns>The saved image entity with ID that can be used in content.</returns>
    Task<ContentImage> GenerateImageAsync(string prompt, string? size = null, string? quality = null, string? model = null);
}
