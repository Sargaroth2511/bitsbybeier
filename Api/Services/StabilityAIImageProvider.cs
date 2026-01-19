using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using bitsbybeier.Api.Configuration;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Image generation provider using Stability AI's APIs (Stable Diffusion 3.5, Stable Image Core/Ultra).
/// Implements IImageGenerationProvider for easy swapping with other providers.
/// </summary>
public class StabilityAIImageProvider : IImageGenerationProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IImageService _imageService;
    private readonly ILogger<StabilityAIImageProvider> _logger;
    private readonly StabilityAIOptions _options;

    public string ProviderName => "Stability AI";

    public StabilityAIImageProvider(
        IHttpClientFactory httpClientFactory,
        IImageService imageService,
        ILogger<StabilityAIImageProvider> logger,
        IOptions<StabilityAIOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _imageService = imageService;
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<ContentImage> GenerateImageAsync(string prompt, string? size = null, string? quality = null, string? model = null)
    {
        _logger.LogInformation("Generating image with Stability AI. Prompt: {Prompt}", prompt);

        // Validate API key
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Stability AI API key is not configured. Please set the STABILITY_AI_API_KEY environment variable.");
        }

        // Determine service and parameters
        var service = model ?? _options.DefaultService;
        var aspectRatio = size ?? _options.DefaultAspectRatio;
        var outputFormat = _options.DefaultOutputFormat; // Always use configured format (png/jpeg/webp)
        
        // Valid style presets for Stability AI
        var validStylePresets = new[] { "3d-model", "analog-film", "anime", "cinematic", "comic-book", 
            "digital-art", "enhance", "fantasy-art", "isometric", "line-art", "low-poly", "neon-punk", 
            "origami", "photographic", "pixel-art", "tile-texture" };
        
        var stylePreset = (!string.IsNullOrWhiteSpace(quality) && validStylePresets.Contains(quality.ToLowerInvariant()))
            ? quality 
            : _options.DefaultStylePreset;
        
        _logger.LogInformation("Using service: {Service}, aspect ratio: {AspectRatio}, format: {Format}, style: {Style}", 
            service, aspectRatio, outputFormat, stylePreset ?? "default");

        // Build request based on service type
        var endpoint = service.ToLowerInvariant() switch
        {
            "ultra" => $"{_options.ApiBaseUrl}/stable-image/generate/ultra",
            "core" => $"{_options.ApiBaseUrl}/stable-image/generate/core",
            "sd3" or "sd3.5" or "stable-diffusion" => $"{_options.ApiBaseUrl}/stable-image/generate/sd3",
            _ => $"{_options.ApiBaseUrl}/stable-image/generate/core" // Default to core
        };

        // Create multipart form data with proper field names
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(prompt), "\"prompt\"");
        content.Add(new StringContent(aspectRatio), "\"aspect_ratio\"");
        content.Add(new StringContent(outputFormat), "\"output_format\"");
        
        // Add model parameter for SD3.5
        if (service.ToLowerInvariant().Contains("sd3"))
        {
            var sd3Model = model ?? _options.DefaultSD3Model;
            content.Add(new StringContent(sd3Model), "\"model\"");
        }
        
        // Add optional style preset (quality parameter for Stability)
        if (!string.IsNullOrWhiteSpace(stylePreset))
        {
            content.Add(new StringContent(stylePreset), "\"style_preset\"");
        }

        _logger.LogDebug("Calling Stability AI endpoint: {Endpoint}", endpoint);

        // Call Stability AI API
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        httpClient.DefaultRequestHeaders.Add("Accept", "image/*"); // Request raw image bytes
        httpClient.Timeout = TimeSpan.FromSeconds(90); // Stability AI can take longer

        byte[] imageBytes;
        try
        {
            var response = await httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("=== Stability AI API ERROR ===");
                _logger.LogError("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogError("Full Response: {Response}", errorContent);
                _logger.LogError("Request was: Service={Service}, AspectRatio={AspectRatio}, OutputFormat={Format}", 
                    service, aspectRatio, outputFormat);
                _logger.LogError("Prompt length: {PromptLength} characters", prompt.Length);
                _logger.LogError("Prompt: {Prompt}", prompt);
                throw new InvalidOperationException($"Stability AI API request failed with status {response.StatusCode}: {errorContent}");
            }

            // Read image bytes directly
            imageBytes = await response.Content.ReadAsByteArrayAsync();
            
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new InvalidOperationException("Stability AI API returned no image data");
            }

            _logger.LogInformation("Image generated successfully. Size: {Size} bytes", imageBytes.Length);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to Stability AI API");
            throw new InvalidOperationException($"Failed to connect to Stability AI API: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Stability AI API request timed out");
            throw new InvalidOperationException("Stability AI API request timed out after 90 seconds", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Stability AI API response");
            throw new InvalidOperationException($"Failed to parse Stability AI API response: {ex.Message}", ex);
        }

        // Save image using existing ImageService
        try
        {
            var description = $"stability-{service}-{DateTime.UtcNow:yyyyMMddHHmmss}.{outputFormat}";
            var contentType = outputFormat switch
            {
                "jpeg" => "image/jpeg",
                "webp" => "image/webp",
                _ => "image/png"
            };
            
            // Convert raw bytes to Base64 for UploadImageAsync
            var base64Data = Convert.ToBase64String(imageBytes);
            var image = await _imageService.UploadImageAsync(base64Data, description, contentType);
            
            _logger.LogInformation("Image saved with ID: {ImageId}", image.Id);
            return image;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save generated image");
            throw new InvalidOperationException($"Failed to save generated image: {ex.Message}", ex);
        }
    }
}
