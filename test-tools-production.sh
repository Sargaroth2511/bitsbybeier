#!/bin/bash

# Quick test to list MCP tools from production server without auth
# (MCP endpoint should allow tools/list without auth for discovery)

echo "========================================"
echo "Listing MCP Tools from Production"
echo "========================================"
echo ""

# Try without auth first (some MCP servers allow tools/list for discovery)
echo "Attempting to list tools..."
curl -s -X POST https://bitsbybeier.de/api/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/list"
  }' | jq '.' 2>/dev/null || echo "Failed - authentication might be required"

echo ""
echo "========================================"
