namespace bitsbybeier.Api.Configuration;

/// <summary>
/// Configuration options for Stability AI image generation.
/// </summary>
public class StabilityAIOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "StabilityAI";
    
    /// <summary>
    /// Stability AI API key. Get yours at https://platform.stability.ai/account/keys
    /// Required for all Stability AI image generation.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Base URL for Stability AI API.
    /// Default: https://api.stability.ai/v2beta
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.stability.ai/v2beta";
    
    /// <summary>
    /// Default service to use.
    /// Options: "core" (fast, 3 credits), "ultra" (photorealistic, 8 credits), "sd3" (Stable Diffusion 3.5, 2.5-6.5 credits)
    /// Default: "core" (best balance of quality and cost)
    /// </summary>
    public string DefaultService { get; set; } = "core";
    
    /// <summary>
    /// Default aspect ratio for generated images.
    /// Options: "1:1" (square), "16:9" (landscape), "9:16" (portrait), "21:9", "2:3", "3:2", "4:5", "5:4", "9:21"
    /// Default: "1:1"
    /// </summary>
    public string DefaultAspectRatio { get; set; } = "1:1";
    
    /// <summary>
    /// Default model for SD3.5 service.
    /// Options: "sd3.5-large" (best quality), "sd3.5-large-turbo" (fast), "sd3.5-medium", "sd3.5-flash" (fastest)
    /// Default: "sd3.5-medium" (good balance)
    /// </summary>
    public string DefaultSD3Model { get; set; } = "sd3.5-medium";
    
    /// <summary>
    /// Default output format.
    /// Options: "png", "jpeg", "webp"
    /// Default: "png"
    /// </summary>
    public string DefaultOutputFormat { get; set; } = "png";
    
    /// <summary>
    /// Default style preset (optional).
    /// Options: "3d-model", "analog-film", "anime", "cinematic", "comic-book", "digital-art", 
    ///          "enhance", "fantasy-art", "isometric", "line-art", "low-poly", "neon-punk", 
    ///          "origami", "photographic", "pixel-art", "tile-texture"
    /// Leave empty for no style preset.
    /// </summary>
    public string? DefaultStylePreset { get; set; }
}
