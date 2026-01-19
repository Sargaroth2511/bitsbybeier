using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Interface for image generation providers.
/// Allows easy swapping between different AI image generation services (OpenAI, Stability AI, etc.)
/// </summary>
public interface IImageGenerationProvider
{
    /// <summary>
    /// Generates an image using the provider's AI model.
    /// </summary>
    /// <param name="prompt">Text description of the image to generate.</param>
    /// <param name="size">Optional size specification (format depends on provider).</param>
    /// <param name="quality">Optional quality specification (format depends on provider).</param>
    /// <param name="model">Optional model specification (format depends on provider).</param>
    /// <returns>The generated ContentImage with image data and metadata.</returns>
    Task<ContentImage> GenerateImageAsync(string prompt, string? size = null, string? quality = null, string? model = null);
    
    /// <summary>
    /// Gets the name of this provider for logging and identification.
    /// </summary>
    string ProviderName { get; }
}
