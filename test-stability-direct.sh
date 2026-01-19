#!/bin/bash

# Direct test of Stability AI API (bypassing our app)
echo "Testing Stability AI API directly..."
echo ""

# Get API key from user secrets
API_KEY=$(dotnet user-secrets list | grep "StabilityAI:ApiKey" | cut -d'=' -f2 | xargs)

if [ -z "$API_KEY" ]; then
    echo "❌ No Stability AI API key found in user secrets"
    exit 1
fi

echo "✅ Found API key: ${API_KEY:0:15}..."
echo ""
echo "Making request to Stability AI API..."
echo ""

# Test Core service
curl -f -X POST "https://api.stability.ai/v2beta/stable-image/generate/core" \
  -H "Authorization: Bearer $API_KEY" \
  -H "Accept: image/*" \
  -F "prompt=A chocolate cake with strawberries" \
  -F "aspect_ratio=1:1" \
  -F "output_format=png" \
  --output /tmp/stability-test.png

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Success! Image saved to /tmp/stability-test.png"
    echo ""
    file /tmp/stability-test.png
    ls -lh /tmp/stability-test.png
else
    echo ""
    echo "❌ API call failed. Check your API key and credits."
fi
