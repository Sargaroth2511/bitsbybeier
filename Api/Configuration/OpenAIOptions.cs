namespace bitsbybeier.Api.Configuration;

/// <summary>
/// Configuration options for OpenAI API integration.
/// </summary>
public class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    /// <summary>
    /// OpenAI API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// OpenAI API base URL (default: https://api.openai.com/v1).
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>
    /// Default model to use for image generation (e.g., dall-e-3, dall-e-2).
    /// </summary>
    public string DefaultModel { get; set; } = "dall-e-3";

    /// <summary>
    /// Default image size (e.g., 1024x1024, 1792x1024, 1024x1792).
    /// </summary>
    public string DefaultSize { get; set; } = "1024x1024";

    /// <summary>
    /// Default image quality (standard or hd).
    /// </summary>
    public string DefaultQuality { get; set; } = "standard";
}
