namespace bitsbybeier.Api.Configuration;

/// <summary>
/// Configuration for image generation service provider selection.
/// </summary>
public class ImageGenerationOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "ImageGeneration";
    
    /// <summary>
    /// Active provider to use for image generation.
    /// Options: "openai" (DALL-E), "stability" (Stable Diffusion)
    /// Default: "openai"
    /// </summary>
    public string Provider { get; set; } = "openai";
}
