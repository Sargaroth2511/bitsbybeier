#!/bin/bash

# Get JWT token
echo "Paste your JWT token:"
read JWT_TOKEN

echo ""
echo "Testing Stability AI via MCP (Core model)..."
echo ""

# Test 1: Stability AI Core
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
        "prompt": "A chocolate cake with strawberries on top, professional food photography",
        "size": "1:1",
        "model": "core"
      }
    }
  }'

echo ""
echo ""
echo "Testing Stability AI SD3.5 Medium model..."
echo ""

# Test 2: SD3.5 Medium
curl -k -X POST https://localhost:5001/api/mcp \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 2,
    "method": "tools/call",
    "params": {
      "name": "generate_image",
      "arguments": {
        "prompt": "A futuristic robot with glowing blue eyes in a dark room",
        "size": "16:9",
        "model": "sd3.5-medium"
      }
    }
  }'

echo ""
echo ""
echo "✅ Tests complete! Check above for image IDs."
