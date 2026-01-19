# Image Generation Provider Configuration

The application now supports multiple AI image generation providers that can be easily swapped:

- **OpenAI DALL-E** (default) - High-quality, creative image generation
- **Stability AI** - Stable Diffusion 3.5, Stable Image Core/Ultra

## Quick Start

### Using OpenAI DALL-E (Default)

```bash
# Development
dotnet user-secrets set "OpenAI:ApiKey" "sk-proj-your-key-here"

# Production (systemd service)
Environment="OPENAI_API_KEY=sk-proj-your-key-here"
```

### Switching to Stability AI

1. Get your API key from https://platform.stability.ai/account/keys

2. Configure the key:

```bash
# Development
dotnet user-secrets set "StabilityAI:ApiKey" "sk-your-stability-key"

# Production
Environment="STABILITY_AI_API_KEY=sk-your-stability-key"
```

3. Switch the provider in `appsettings.json`:

```json
{
  "ImageGeneration": {
    "Provider": "stability"
  }
}
```

## Configuration Options

### OpenAI DALL-E Options

```json
{
  "OpenAI": {
    "ApiKey": "${OPENAI_API_KEY}",
    "ApiBaseUrl": "https://api.openai.com/v1",
    "DefaultModel": "dall-e-3",
    "DefaultSize": "1024x1024",
    "DefaultQuality": "standard"
  }
}
```

- **Model**: `dall-e-3` (best quality) or `dall-e-2` (faster)
- **Size**: `1024x1024`, `1792x1024`, `1024x1792`
- **Quality**: `standard` (fast) or `hd` (higher detail)

### Stability AI Options

```json
{
  "StabilityAI": {
    "ApiKey": "${STABILITY_AI_API_KEY}",
    "ApiBaseUrl": "https://api.stability.ai/v2beta",
    "DefaultService": "core",
    "DefaultAspectRatio": "1:1",
    "DefaultSD3Model": "sd3.5-medium",
    "DefaultOutputFormat": "png",
    "DefaultStylePreset": ""
  }
}
```

#### Services

- **`core`** (3 credits) - Fast and affordable, great for rapid iteration
- **`ultra`** (8 credits) - Highest quality photorealistic output
- **`sd3`** (2.5-6.5 credits) - Stable Diffusion 3.5 models

#### Aspect Ratios

`1:1`, `16:9`, `9:16`, `21:9`, `9:21`, `2:3`, `3:2`, `4:5`, `5:4`

#### SD3.5 Models (when service = "sd3")

- **`sd3.5-large`** (6.5 credits) - Best quality, 8B parameters
- **`sd3.5-large-turbo`** (4 credits) - Fast version of large
- **`sd3.5-medium`** (3.5 credits) - Balanced quality/speed
- **`sd3.5-flash`** (2.5 credits) - Fastest generation

#### Style Presets (Optional)

`3d-model`, `analog-film`, `anime`, `cinematic`, `comic-book`, `digital-art`, 
`enhance`, `fantasy-art`, `isometric`, `line-art`, `low-poly`, `neon-punk`, 
`origami`, `photographic`, `pixel-art`, `tile-texture`

#### Output Formats

`png`, `jpeg`, `webp`

## MCP Tool Usage

ChatGPT can override defaults when calling the `generate_image` tool:

### OpenAI Examples

```typescript
// Default DALL-E 3
generate_image("A Viking warrior")

// High detail DALL-E 3
generate_image("A Viking warrior", size="1024x1792", quality="hd")

// Fast DALL-E 2
generate_image("A Viking warrior", model="dall-e-2")
```

### Stability AI Examples

```typescript
// Default (Core service, 1:1)
generate_image("A Viking warrior")

// Ultra service with landscape
generate_image("A Viking warrior", size="16:9", model="ultra")

// SD3.5 Large with portrait
generate_image("A Viking warrior", size="9:16", model="sd3.5-large")

// With style preset
generate_image("A Viking warrior", size="1:1", quality="photographic", model="core")
```

## Cost Comparison

### OpenAI DALL-E

- **DALL-E 3** (1024x1024): ~$0.040 per image
- **DALL-E 3** (1792x1024): ~$0.080 per image
- **DALL-E 2** (1024x1024): ~$0.020 per image

### Stability AI (Credit-based, ~$0.01 per 100 credits)

- **Core**: 3 credits = ~$0.0003 per image
- **Ultra**: 8 credits = ~$0.0008 per image
- **SD3.5 Medium**: 3.5 credits = ~$0.00035 per image
- **SD3.5 Large**: 6.5 credits = ~$0.00065 per image

**Stability AI is ~100x cheaper than DALL-E!**

## Testing

```bash
# Test OpenAI
curl -X POST http://localhost:5000/api/mcp \
  -H "Authorization: Bearer YOUR_JWT" \
  -d '{"method": "tools/call", "params": {"name": "generate_image", "arguments": {"prompt": "A chocolate cake"}}}'

# Test Stability AI (after switching provider)
curl -X POST http://localhost:5000/api/mcp \
  -H "Authorization: Bearer YOUR_JWT" \
  -d '{"method": "tools/call", "params": {"name": "generate_image", "arguments": {"prompt": "A chocolate cake", "model": "sd3"}}}'
```

## Troubleshooting

### OpenAI Errors

- **401 Unauthorized**: API key not set or invalid
- **400 Bad Request**: Invalid parameters (check size, model)
- **429 Rate Limit**: Too many requests

### Stability AI Errors

- **403 Forbidden**: Content moderation flagged your prompt
- **422 Unprocessable**: Invalid parameter combination
- **429 Rate Limit**: >150 requests in 10 seconds

Check server logs for detailed error messages:
```bash
sudo journalctl -u dotnet-web.service -f | grep -i "image\|error"
```

## Architecture

```
┌─────────────────────────────────────┐
│  MCP generate_image Tool            │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  ImageGenerationService             │
│  (Provider Selector)                │
└──────────────┬──────────────────────┘
               │
      ┌────────┴────────┐
      │                 │
      ▼                 ▼
┌──────────────┐  ┌──────────────────┐
│ OpenAI       │  │ Stability AI     │
│ Provider     │  │ Provider         │
└──────┬───────┘  └────────┬─────────┘
       │                   │
       ▼                   ▼
┌──────────────────────────────────────┐
│  ImageService                        │
│  (Download, Optimize, Save to DB)   │
└──────────────────────────────────────┘
```

## Future Providers

To add a new provider (e.g., Midjourney, RunPod):

1. Create `NewProviderImageProvider.cs` implementing `IImageGenerationProvider`
2. Add configuration class `NewProviderOptions.cs`
3. Register in `Program.cs`
4. Update `ImageGenerationService` selector
5. Add to `appsettings.json`

The architecture is designed for easy extension!
