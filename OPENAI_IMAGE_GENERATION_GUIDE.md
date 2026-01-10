# OpenAI DALL-E Image Generation Integration

This document describes the OpenAI DALL-E integration for automatic image generation in the CMS.

## Overview

The MCP server now supports automatic image generation using OpenAI's DALL-E API. AI agents can request image generation directly through the `generate_image` tool, which automatically:
1. Calls OpenAI DALL-E API with the provided prompt
2. Downloads the generated image
3. Saves it to the server's database
4. Returns the image ID for use in content

## Setup Instructions

### 1. Obtain OpenAI API Key

1. Go to [OpenAI Platform](https://platform.openai.com/)
2. Sign in or create an account
3. Navigate to API Keys section
4. Create a new API key
5. Copy the key (you won't be able to see it again)

### 2. Configure the Application

Set the `OPENAI_API_KEY` environment variable:

**Linux/macOS:**
```bash
export OPENAI_API_KEY='your-api-key-here'
```

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY = "your-api-key-here"
```

**Windows (Command Prompt):**
```cmd
set OPENAI_API_KEY=your-api-key-here
```

**Docker/Production:**
Add to your `.env` file or container environment:
```
OPENAI_API_KEY=your-api-key-here
```

### 3. Verify Configuration

The configuration is loaded from `appsettings.json`:

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

You can customize these values in `appsettings.json` or `appsettings.Development.json`.

## Usage

### New Workflow for AI Agents

When an AI agent (like ChatGPT) needs to create content with images:

**Step 1: Generate Images**
```javascript
// Generate first image
await client.CallToolAsync("generate_image", {
    prompt: "A delicious chocolate cake with frosting",
    size: "1024x1024",
    quality: "standard"
});
// Returns: "Image generated successfully. ID: 5, ..."

// Generate second image
await client.CallToolAsync("generate_image", {
    prompt: "A modern bakery storefront",
    size: "1792x1024"
});
// Returns: "Image generated successfully. ID: 6, ..."
```

**Step 2: Create Content with Generated Images**
```javascript
await client.CallToolAsync("create_content", {
    author: "Chef Marie",
    title: "Perfect Chocolate Cake",
    content: `# Perfect Chocolate Cake

![Chocolate Cake](https://bitsbybeier.de/api/images/5)

This amazing cake...

## Visit Our Bakery

![Our Store](https://bitsbybeier.de/api/images/6)`,
    imageIds: "5,6",
    draft: true
});
```

### Tool Parameters

#### generate_image

- **prompt** (required): Detailed description of the image to generate
- **size** (optional): Image dimensions
  - `"1024x1024"` - Square (default)
  - `"1792x1024"` - Landscape
  - `"1024x1792"` - Portrait
- **quality** (optional): Quality setting
  - `"standard"` - Standard quality, faster, cheaper (default)
  - `"hd"` - High definition, more detailed
- **model** (optional): DALL-E model
  - `"dall-e-3"` - Latest model, better quality (default)
  - `"dall-e-2"` - Older model, faster

## Cost Considerations

OpenAI charges per image generated. Pricing varies by model, size, and quality:
- **DALL-E 3** generates higher quality images but costs more
- **DALL-E 2** is more economical for basic needs
- **HD quality** costs more than standard quality
- Larger images (1792x1024) may cost more than smaller ones

**Important**: Prices are subject to change. Check the [OpenAI Pricing Page](https://openai.com/api/pricing/) for current rates.

As a reference (accurate as of January 2024, may be outdated):
- DALL-E 3 Standard 1024x1024: ~$0.04 per image
- DALL-E 3 HD 1024x1024: ~$0.08 per image
- DALL-E 2 1024x1024: ~$0.02 per image

## Technical Details

### Service Architecture

```
AI Agent (ChatGPT)
    ↓ (calls generate_image via MCP)
ContentMcpTools
    ↓
OpenAIImageService
    ↓ (HTTP POST)
OpenAI DALL-E API
    ↓ (returns temporary URL)
OpenAIImageService
    ↓ (downloads image)
ImageService
    ↓ (saves to database)
Database (ContentImages table)
```

### Implementation

- **Service**: `OpenAIImageService` (`Api/Services/OpenAIImageService.cs`)
- **Interface**: `IOpenAIImageService` (`Api/Services/IOpenAIImageService.cs`)
- **Configuration**: `OpenAIOptions` (`Api/Configuration/OpenAIOptions.cs`)
- **MCP Tool**: `generate_image` in `ContentMcpTools` (`Api/Mcp/ContentMcpServer.cs`)

### Features

- ✅ Automatic image download from DALL-E
- ✅ Image optimization and resizing (via ImageService)
- ✅ Database storage with metadata
- ✅ Unique filename generation
- ✅ Error handling and logging
- ✅ Configurable defaults
- ✅ Support for both DALL-E 3 and DALL-E 2
- ✅ Multiple size and quality options

## Troubleshooting

### "OpenAI API key is not configured"

**Problem**: The API key environment variable is not set.

**Solution**: Set the `OPENAI_API_KEY` environment variable and restart the application.

### "OpenAI API request failed with status 401"

**Problem**: Invalid API key.

**Solution**: Verify your API key is correct and has not been revoked. Generate a new one if needed.

### "OpenAI API request failed with status 429"

**Problem**: Rate limit exceeded or insufficient credits.

**Solution**: 
- Check your OpenAI account has sufficient credits
- Wait before making more requests
- Consider upgrading your OpenAI plan

### "OpenAI API request timed out"

**Problem**: Image generation took longer than 60 seconds.

**Solution**: This is rare but can happen. Retry the request. The timeout is set to 60 seconds in the code.

### "Failed to download and save generated image"

**Problem**: Error downloading the image from OpenAI's temporary URL.

**Solution**: 
- Check network connectivity
- The DALL-E URLs are temporary (expire after a few minutes) - this shouldn't happen as we download immediately
- Check logs for detailed error message

## Security

- ✅ API key stored in environment variables (never in code)
- ✅ API key never logged or exposed
- ✅ Images validated before saving
- ✅ Input sanitization
- ✅ Error messages don't expose sensitive data

## Testing

See `/tmp/test-openai-integration.sh` for a testing guide.

Quick test:
```bash
# 1. Set API key
export OPENAI_API_KEY='your-key'

# 2. Start application
dotnet run

# 3. Use MCP client to test
# Call generate_image with a test prompt
# Verify image is saved and ID is returned
```

## Additional Resources

- [OpenAI DALL-E Documentation](https://platform.openai.com/docs/guides/images)
- [MCP Content API README](./MCP_CONTENT_API_README.md)
- [OpenAI API Reference](https://platform.openai.com/docs/api-reference/images)
