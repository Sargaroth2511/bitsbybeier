using Microsoft.Extensions.Options;
using bitsbybeier.Api.Configuration;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service that selects and delegates to the configured image generation provider.
/// Maintains backward compatibility with IOpenAIImageService interface.
/// </summary>
public class ImageGenerationService : IOpenAIImageService
{
    private readonly IEnumerable<IImageGenerationProvider> _providers;
    private readonly ILogger<ImageGenerationService> _logger;
    private readonly ImageGenerationOptions _options;

    public ImageGenerationService(
        IEnumerable<IImageGenerationProvider> providers,
        ILogger<ImageGenerationService> logger,
        IOptions<ImageGenerationOptions> options)
    {
        _providers = providers;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<ContentImage> GenerateImageAsync(string prompt, string? size = null, string? quality = null, string? model = null)
    {
        IImageGenerationProvider? provider = null;

        // Smart provider selection: Check if model name indicates which provider to use
        if (!string.IsNullOrWhiteSpace(model))
        {
            var modelLower = model.ToLowerInvariant();
            
            // Detect DALL-E models
            if (modelLower.StartsWith("dall-e"))
            {
                provider = _providers.FirstOrDefault(p => p is OpenAIImageProvider);
                _logger.LogInformation("Model '{Model}' detected as OpenAI DALL-E. Selecting OpenAI provider.", model);
            }
            // Detect Stability AI models/services
            else if (modelLower == "ultra" || modelLower == "core" || modelLower == "sd3" ||
                     modelLower.StartsWith("sd3.") || modelLower.Contains("stable") || 
                     modelLower.StartsWith("sd-") || modelLower.StartsWith("sdxl"))
            {
                provider = _providers.FirstOrDefault(p => p is StabilityAIImageProvider);
                _logger.LogInformation("Model '{Model}' detected as Stability AI. Selecting Stability provider.", model);
            }
        }

        // Fall back to configured provider if model doesn't specify
        if (provider == null)
        {
            var providerName = _options.Provider.ToLowerInvariant();
            _logger.LogInformation("No provider detected from model. Using configured provider: {Provider}", providerName);

            provider = providerName switch
            {
                "openai" => _providers.FirstOrDefault(p => p is OpenAIImageProvider),
                "stability" or "stable-diffusion" or "stabilitya" => _providers.FirstOrDefault(p => p is StabilityAIImageProvider),
                _ => null
            };
        }

        // Final fallback: use first available provider
        if (provider == null)
        {
            _logger.LogWarning("Configured provider not found. Falling back to first available provider.");
            provider = _providers.FirstOrDefault();
        }

        if (provider == null)
        {
            throw new InvalidOperationException("No image generation providers are configured. Please configure at least one provider (OpenAI or Stability AI).");
        }

        _logger.LogInformation("Using provider: {ProviderName}", provider.ProviderName);
        return await provider.GenerateImageAsync(prompt, size, quality, model);
    }
}
