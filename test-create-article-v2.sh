#!/bin/bash

echo "Paste your JWT token:"
read JWT_TOKEN

echo ""
echo "Creating test article with image ID 2..."
echo ""

curl -k -X POST https://localhost:5001/api/mcp \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 3,
    "method": "tools/call",
    "params": {
      "name": "create_content",
      "arguments": {
        "author": "AI Test v2",
        "title": "Fixed Preview: Chocolate Cake",
        "subtitle": "Now with correct localhost URLs",
        "content": "# Amazing Chocolate Cake\n\nCheck out this delicious chocolate cake:\n\n![Chocolate Cake](https://localhost:5001/api/images/2)\n\nThis image should now display in the preview!",
        "imageIds": "2",
        "draft": true
      }
    }
  }'

echo ""
echo ""
echo "View at http://localhost:4300/drafts"
