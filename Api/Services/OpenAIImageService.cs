using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using bitsbybeier.Api.Configuration;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for generating images using OpenAI's DALL-E API.
/// </summary>
public class OpenAIImageService : IOpenAIImageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IImageService _imageService;
    private readonly ILogger<OpenAIImageService> _logger;
    private readonly OpenAIOptions _options;

    public OpenAIImageService(
        IHttpClientFactory httpClientFactory,
        IImageService imageService,
        ILogger<OpenAIImageService> logger,
        IOptions<OpenAIOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _imageService = imageService;
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc/>
    public async Task<ContentImage> GenerateImageAsync(string prompt, string? size = null, string? quality = null, string? model = null)
    {
        _logger.LogInformation("Generating image with OpenAI DALL-E. Prompt: {Prompt}", prompt);

        // Validate API key is configured
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured. Please set the OpenAI:ApiKey configuration value or OPENAI_API_KEY environment variable.");
        }

        // Use provided values or defaults from configuration
        var imageSize = size ?? _options.DefaultSize;
        var imageQuality = quality ?? _options.DefaultQuality;
        var imageModel = model ?? _options.DefaultModel;

        _logger.LogInformation("Using model: {Model}, size: {Size}, quality: {Quality}", imageModel, imageSize, imageQuality);

        // Create request payload for OpenAI API
        var requestPayload = new
        {
            model = imageModel,
            prompt = prompt,
            n = 1, // Generate 1 image
            size = imageSize,
            quality = imageQuality,
            response_format = "url" // Get URL instead of base64 for efficiency
        };

        var jsonContent = JsonSerializer.Serialize(requestPayload);
        _logger.LogDebug("OpenAI request payload: {Payload}", jsonContent);

        // Call OpenAI API
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
        httpClient.Timeout = TimeSpan.FromSeconds(60); // DALL-E can take time to generate

        string imageUrl;
        string revisedPrompt;

        try
        {
            var response = await httpClient.PostAsync(
                $"{_options.ApiBaseUrl}/images/generations",
                new StringContent(jsonContent, Encoding.UTF8, "application/json"));

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("OpenAI API response: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API error: {StatusCode} - {Response}", response.StatusCode, responseContent);
                throw new InvalidOperationException($"OpenAI API request failed with status {response.StatusCode}: {responseContent}");
            }

            // Parse response to get image URL
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var data = jsonDoc.RootElement.GetProperty("data");
            
            if (data.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("OpenAI API returned no images");
            }

            var firstImage = data[0];
            imageUrl = firstImage.GetProperty("url").GetString() 
                ?? throw new InvalidOperationException("OpenAI API returned no image URL");
            
            // DALL-E 3 returns a revised_prompt showing what it actually generated
            revisedPrompt = firstImage.TryGetProperty("revised_prompt", out var rp) 
                ? rp.GetString() ?? prompt 
                : prompt;

            _logger.LogInformation("Image generated successfully. URL: {Url}", imageUrl);
            if (revisedPrompt != prompt)
            {
                _logger.LogInformation("Revised prompt: {RevisedPrompt}", revisedPrompt);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to OpenAI API");
            throw new InvalidOperationException($"Failed to connect to OpenAI API: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "OpenAI API request timed out");
            throw new InvalidOperationException("OpenAI API request timed out after 60 seconds", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse OpenAI API response");
            throw new InvalidOperationException("Failed to parse OpenAI API response", ex);
        }

        // Download and save the image using existing ImageService
        try
        {
            // Generate a meaningful filename based on the prompt
            var fileName = GenerateFileName(prompt);
            
            _logger.LogInformation("Downloading generated image from OpenAI and saving to server. FileName: {FileName}", fileName);
            
            var savedImage = await _imageService.ImportImageFromUrlAsync(imageUrl, fileName);
            
            _logger.LogInformation("Image saved successfully with ID: {ImageId}, Size: {FileSize} bytes", 
                savedImage.Id, savedImage.FileSize);

            return savedImage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download and save generated image");
            throw new InvalidOperationException($"Failed to download and save generated image: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates a filename from the prompt by taking first few words and sanitizing.
    /// </summary>
    private string GenerateFileName(string prompt)
    {
        // Take first 5 words of prompt, replace spaces with hyphens, remove special chars
        var words = prompt.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(5);
        var sanitized = string.Join("-", words)
            .ToLowerInvariant()
            .Replace(",", "")
            .Replace(".", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("'", "")
            .Replace("\"", "");

        // Limit length and add timestamp for uniqueness
        if (sanitized.Length > 30)
        {
            sanitized = sanitized.Substring(0, 30);
        }

        return $"dalle-{sanitized}-{DateTime.UtcNow:yyyyMMddHHmmss}.png";
    }
}
