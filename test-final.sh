#!/bin/bash

echo "Paste your JWT token:"
read JWT_TOKEN

curl -k -X POST https://localhost:5001/api/mcp \
  -H "Authorization: Bearer $JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": 4,
    "method": "tools/call",
    "params": {
      "name": "create_content",
      "arguments": {
        "author": "AI Test Final",
        "title": "Working Preview: Chocolate Cake",
        "subtitle": "With relative URLs that work through Angular proxy",
        "content": "# Amazing Chocolate Cake\n\nThis should display correctly:\n\n![Chocolate Cake](/api/images/3)\n\nImage loads through Angular proxy!",
        "imageIds": "3",
        "draft": true
      }
    }
  }'

echo -e "\n\nGo to http://localhost:4300/drafts and check 'Working Preview: Chocolate Cake'"
