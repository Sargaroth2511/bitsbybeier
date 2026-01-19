#!/bin/bash

# Test Stability AI image generation
# This tests the generate_image tool with a Stability AI model

echo "Testing Stability AI image generation..."
echo ""

# First, login to get JWT token
echo "Step 1: Getting JWT token..."
LOGIN_RESPONSE=$(curl -s -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@bitsbybeier.de","password":"Admin123!"}' \
  -k)

TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
    echo "❌ Failed to get token. Response:"
    echo $LOGIN_RESPONSE
    exit 1
fi

echo "✅ Got JWT token"
echo ""

# Test 1: Stability AI Core (fast, cheap)
echo "Step 2: Testing Stability AI Core model..."
echo "Generating image with prompt: 'A chocolate cake with strawberries'"
echo ""

CORE_RESPONSE=$(curl -s -X POST https://localhost:5001/api/mcp \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
      "name": "generate_image",
      "arguments": {
        "prompt": "A chocolate cake with strawberries on top",
        "model": "core",
        "size": "1:1"
      }
    },
    "id": 1
  }' \
  -k)

echo "Response:"
echo $CORE_RESPONSE | jq '.' 2>/dev/null || echo $CORE_RESPONSE
echo ""
echo "---"
echo ""

# Test 2: Stability AI SD3.5 Medium (better quality)
echo "Step 3: Testing Stability AI SD3.5 Medium model..."
echo "Generating image with prompt: 'A futuristic robot'"
echo ""

SD3_RESPONSE=$(curl -s -X POST https://localhost:5001/api/mcp \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
      "name": "generate_image",
      "arguments": {
        "prompt": "A futuristic robot with glowing eyes",
        "model": "sd3.5-medium",
        "size": "16:9"
      }
    },
    "id": 2
  }' \
  -k)

echo "Response:"
echo $SD3_RESPONSE | jq '.' 2>/dev/null || echo $SD3_RESPONSE
echo ""

echo "✅ Tests complete!"
echo ""
echo "Check the responses above for image IDs."
echo "You can view generated images at: https://localhost:5001/api/images/{ID}"
