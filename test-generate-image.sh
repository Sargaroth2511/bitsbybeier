#!/bin/bash

# Get JWT token (you'll paste yours)
echo "Paste your JWT token from browser (from localStorage or from login response):"
read JWT_TOKEN

echo ""
echo "Testing generate_image via MCP..."
echo ""

# MCP protocol call to generate_image
curl -k -X POST https://localhost:5001/api/mcp \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/call",
    "params": {
      "name": "generate_image",
      "arguments": {
        "prompt": "A delicious chocolate cake with chocolate frosting and sprinkles, professional food photography",
        "size": "1024x1024",
        "quality": "standard"
      }
    }
  }'

echo ""
echo "If successful, you should see an image ID. Use that ID in create_content!"
