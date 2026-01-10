# Quick Start: OpenAI Image Generation 🚀

## What's New?

AI agents can now generate images directly through the MCP server using OpenAI's DALL-E API!

## Setup (One-Time)

```bash
# 1. Get your API key from https://platform.openai.com/
# 2. Set environment variable
export OPENAI_API_KEY='sk-...'

# 3. Start the application
dotnet run
```

## How It Works

### Old Way ❌ (Manual)
1. Generate image in ChatGPT → Get URL
2. Call `import_image_from_url` with URL
3. Get image ID
4. Create content with image ID

### New Way ✅ (Automatic)
1. Call `generate_image` with prompt → Get image ID
2. Create content with image ID

**Result**: Fewer steps, automatic storage, consistent workflow!

## Example Usage

### For LLMs (ChatGPT, Claude, etc.)

```javascript
// Step 1: Generate images
await generate_image({
  prompt: "A chocolate cake with frosting",
  size: "1024x1024",
  quality: "standard"
});
// Returns: ID: 5

await generate_image({
  prompt: "A modern bakery interior",
  size: "1792x1024"
});
// Returns: ID: 6

// Step 2: Create content with images
await create_content({
  author: "Chef Marie",
  title: "Perfect Chocolate Cake Recipe",
  content: `# Perfect Chocolate Cake

![Chocolate Cake](https://bitsbybeier.de/api/images/5)

This amazing recipe...

## Visit Us

![Our Bakery](https://bitsbybeier.de/api/images/6)`,
  imageIds: "5,6",
  draft: true
});
```

## MCP Tool: generate_image

**Parameters:**
- `prompt` *(required)*: Description of image to generate
- `size` *(optional)*: "1024x1024" (square), "1792x1024" (landscape), "1024x1792" (portrait)
- `quality` *(optional)*: "standard" (faster) or "hd" (better quality)
- `model` *(optional)*: "dall-e-3" (better) or "dall-e-2" (cheaper)

**Returns:** Image ID and markdown URL format

## Configuration

### Default Settings (in appsettings.json)
- Model: `dall-e-3`
- Size: `1024x1024`
- Quality: `standard`

### Customization
Edit `appsettings.json` to change defaults:

```json
{
  "OpenAI": {
    "DefaultModel": "dall-e-2",
    "DefaultSize": "1024x1024",
    "DefaultQuality": "standard"
  }
}
```

## Troubleshooting

### "OpenAI API key is not configured"
→ Set the `OPENAI_API_KEY` environment variable

### "OpenAI API request failed with status 401"
→ Check your API key is valid

### "OpenAI API request failed with status 429"
→ Rate limit or no credits. Check your OpenAI account

## Costs

OpenAI charges per image:
- **DALL-E 3**: Higher quality, ~$0.04-0.08 per image
- **DALL-E 2**: Budget option, ~$0.02 per image

Check latest pricing: https://openai.com/api/pricing/

## More Information

- **Full Setup Guide**: `OPENAI_IMAGE_GENERATION_GUIDE.md`
- **Implementation Details**: `OPENAI_INTEGRATION_SUMMARY.md`
- **API Documentation**: `MCP_CONTENT_API_README.md`

## Architecture

```
LLM → generate_image tool → OpenAI API → Auto-save → Returns ID
```

Simple, automatic, reliable! 🎉

---

**Need Help?**
- Check `OPENAI_IMAGE_GENERATION_GUIDE.md` for detailed setup
- Review `MCP_CONTENT_API_README.md` for API reference
- See examples in documentation files
