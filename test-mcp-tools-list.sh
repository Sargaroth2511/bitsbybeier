#!/bin/bash

# Test script to check what MCP tools ChatGPT can see
# This calls the MCP server to list available tools

echo "========================================"
echo "Testing MCP Server - List Available Tools"
echo "========================================"
echo ""

# Get JWT token from environment or parameter
if [ -z "$JWT_TOKEN" ]; then
    echo "ERROR: JWT_TOKEN environment variable not set!"
    echo ""
    echo "Usage:"
    echo "  export JWT_TOKEN='your-jwt-token-here'"
    echo "  ./test-mcp-tools-list.sh"
    echo ""
    echo "Or:"
    echo "  JWT_TOKEN='your-token' ./test-mcp-tools-list.sh"
    exit 1
fi

echo "Connecting to: https://bitsbybeier.de/api/mcp"
echo ""

# Call MCP server to list tools (using MCP protocol)
curl -X POST https://bitsbybeier.de/api/mcp \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/list"
  }' | jq '.'

echo ""
echo "========================================"
echo "Test complete!"
echo "========================================"
