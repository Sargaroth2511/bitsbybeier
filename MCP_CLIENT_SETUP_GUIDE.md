# MCP Client Setup Guide

This guide explains how to connect your local ChatGPT app (or any MCP-compatible client) to the BitsbyBeier MCP server for testing in development mode.

## Prerequisites

- The BitsbyBeier application running locally
- Admin user credentials
- MCP-compatible client (Claude Desktop, custom client, etc.)

## Quick Start

### 1. Start the Application

```bash
cd /home/sargaroth/bitsbybeier
dotnet run
```

The server will be available at:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

### 2. Get an Authentication Token

The MCP server requires authentication with Admin role. You need to obtain a JWT token first.

#### Option A: Using Google OAuth (Recommended)

1. Open your browser and navigate to `https://localhost:5001`
2. Click on "Sign in with Google"
3. After successful authentication, open the browser's Developer Tools (F12)
4. Go to the Console tab
5. Type: `localStorage.getItem('jwt_token')`
6. Copy the token (without quotes)

#### Option B: Using curl with Google ID Token

```bash
curl -k -X POST https://localhost:5001/api/auth/google-login \
  -H "Content-Type: application/json" \
  -d '{"idToken": "YOUR_GOOGLE_ID_TOKEN"}'
```

Response:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-01-04T12:00:00Z",
  "user": {
    "id": 1,
    "email": "your@email.com",
    "displayName": "Your Name",
    "role": "Admin"
  }
}
```

### 3. Test the MCP Server Connection

Verify the server is accessible:

```bash
curl -k -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  https://localhost:5001/api/mcp/info
```

Expected response:
```json
{
  "name": "BitsbyBeier Content MCP Server",
  "version": "1.0.0",
  "description": "MCP server for content management operations...",
  "tools": [
    {
      "name": "CreateContentAsync",
      "description": "Creates a new content item in the CMS...",
      "parameters": { ... }
    }
  ]
}
```

## Client Configuration

### For Claude Desktop App

**Location of config file:**
- **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
- **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Linux**: `~/.config/Claude/claude_desktop_config.json`

**Configuration:**

```json
{
  "mcpServers": {
    "bitsbybeier-content": {
      "command": "npx",
      "args": [
        "-y",
        "@modelcontextprotocol/server-fetch@latest",
        "https://localhost:5001/api/mcp"
      ],
      "env": {
        "MCP_AUTH_TOKEN": "YOUR_JWT_TOKEN_HERE",
        "NODE_TLS_REJECT_UNAUTHORIZED": "0"
      }
    }
  }
}
```

**Note**: Replace `YOUR_JWT_TOKEN_HERE` with your actual JWT token from step 2.

### For Custom MCP Client (HTTP Transport)

If you're building a custom MCP client:

```javascript
const client = new MCPClient({
  transport: {
    type: 'http',
    url: 'https://localhost:5001/api/mcp',
    headers: {
      'Authorization': 'Bearer YOUR_JWT_TOKEN_HERE',
      'Content-Type': 'application/json'
    },
    tls: {
      rejectUnauthorized: false  // Only for development!
    }
  }
});

// Connect and list tools
await client.connect();
const tools = await client.listTools();
console.log(tools);
```

### For Custom MCP Client (stdio Transport)

If using stdio-based transport:

```json
{
  "mcpServers": {
    "bitsbybeier": {
      "command": "bash",
      "args": [
        "-c",
        "curl -k -H 'Authorization: Bearer YOUR_JWT_TOKEN' -H 'Content-Type: application/json' -X POST -d @- https://localhost:5001/api/mcp/tools"
      ]
    }
  }
}
```

## Development Mode Setup

### Trust the Development Certificate (Recommended)

To avoid SSL certificate warnings:

```bash
dotnet dev-certs https --trust
```

After trusting the cert, you can remove `NODE_TLS_REJECT_UNAUTHORIZED=0` from your config.

### Or: Disable SSL Verification (Development Only)

If you prefer not to trust the certificate, use these environment variables:

**Node.js/npm clients:**
```bash
NODE_TLS_REJECT_UNAUTHORIZED=0
```

**Python clients:**
```python
import ssl
ssl._create_default_https_context = ssl._create_unverified_context
```

**curl:**
```bash
curl -k  # -k flag disables certificate verification
```

## Using the MCP Server

### Available Tools

#### CreateContentAsync

Creates new content in the CMS.

**Parameters:**
- `author` (string, required): Author name (max 200 chars)
- `title` (string, required): Content title (max 500 chars)
- `content` (string, required): Main content text (supports Markdown)
- `subtitle` (string, optional): Subtitle or summary (max 1000 chars)
- `draft` (boolean, optional, default: true): Create as draft

**Example Usage in Claude:**

```
Can you create a new blog post about TypeScript best practices?
Use the CreateContentAsync tool with:
- Author: Johannes Beier
- Title: TypeScript Best Practices for 2026
- Content: A comprehensive guide with examples...
- Draft: true
```

**Example curl request:**

```bash
curl -k -X POST https://localhost:5001/api/mcp/tools \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CreateContentAsync",
    "arguments": {
      "request": {
        "author": "Johannes Beier",
        "title": "TypeScript Best Practices",
        "content": "# Introduction\n\nTypeScript has become...",
        "subtitle": "A guide for modern developers",
        "draft": true
      }
    }
  }'
```

**Response:**

```json
{
  "content": [
    {
      "type": "text",
      "text": "Content created successfully. ID: 5, Title: TypeScript Best Practices, Draft: True, Created: 2026-01-04 10:30:00 UTC"
    }
  ]
}
```

## Troubleshooting

### Issue: "401 Unauthorized" error

**Solution**: Your JWT token is invalid or expired.
- JWT tokens expire after 30 days (configured in appsettings.json)
- Get a new token using the authentication steps above

### Issue: "403 Forbidden" error

**Solution**: Your user doesn't have Admin role.
- Only Admin users can access MCP endpoints
- Check your user role in the database or via `/api/auth/me` endpoint

### Issue: SSL Certificate errors

**Solutions:**
1. Trust the development certificate: `dotnet dev-certs https --trust`
2. Or use `NODE_TLS_REJECT_UNAUTHORIZED=0` (dev only)
3. Or use `curl -k` flag to skip verification

### Issue: "Connection refused" error

**Solutions:**
1. Ensure the application is running: `dotnet run`
2. Check if the correct port is being used (5001 for HTTPS, 5000 for HTTP)
3. Verify firewall settings allow local connections

### Issue: "No MCP tools available"

**Solution**: Check that:
1. You're using the correct endpoint: `/api/mcp/info`
2. Your Authorization header is correctly formatted
3. The JWT token is valid and from an Admin user

## Security Notes

### For Development

- Using `NODE_TLS_REJECT_UNAUTHORIZED=0` is acceptable for local development
- Self-signed certificates are fine for localhost testing
- JWT tokens stored in browser localStorage are acceptable for dev

### For Production

When deploying to production:

1. **Never** disable SSL certificate verification
2. Use proper SSL/TLS certificates (Let's Encrypt, etc.)
3. Store JWT tokens securely (httpOnly cookies, secure storage)
4. Configure proper CORS policies
5. Use environment-specific configuration
6. Consider implementing token refresh mechanisms
7. Audit MCP tool usage and access logs

## API Reference

### MCP Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/mcp/info` | GET | Required | Get server info and tool list |
| `/api/mcp/tools` | POST | Required | Execute an MCP tool |

### Authentication Endpoints

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/auth/google-login` | POST | None | Authenticate with Google |
| `/api/auth/me` | GET | Required | Get current user info |

### Content Endpoints (Non-MCP)

| Endpoint | Method | Auth | Description |
|----------|--------|------|-------------|
| `/api/cms/content` | GET | Admin | List all content |
| `/api/cms/content` | POST | Admin | Create content |
| `/api/cms/content/{id}` | PUT | Admin | Update content |
| `/api/cms/content/{id}` | DELETE | Admin | Delete content |
| `/api/cms/content/drafts` | GET | Admin | List draft content |
| `/api/cms/content/public` | GET | None | List public content |

## Examples

### Example 1: Create a Simple Article

```bash
curl -k -X POST https://localhost:5001/api/mcp/tools \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CreateContentAsync",
    "arguments": {
      "request": {
        "author": "Jane Doe",
        "title": "Hello World",
        "content": "This is my first post!",
        "draft": true
      }
    }
  }'
```

### Example 2: Create Article with Markdown

```bash
curl -k -X POST https://localhost:5001/api/mcp/tools \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "CreateContentAsync",
    "arguments": {
      "request": {
        "author": "Johannes Beier",
        "title": "Markdown Guide",
        "subtitle": "Learn Markdown basics",
        "content": "# Introduction\n\n**Markdown** is a *lightweight* markup language.\n\n## Features\n\n- Easy to read\n- Easy to write\n- Widely supported",
        "draft": false
      }
    }
  }'
```

### Example 3: Using in Claude Desktop

Once configured, simply ask Claude:

```
Please create a blog post about JavaScript async/await patterns. 
Make it a draft with author "Johannes Beier".
```

Claude will automatically use the `CreateContentAsync` tool with the MCP server.

## Additional Resources

- [MCP Content API README](MCP_CONTENT_API_README.md) - Detailed API documentation
- [Authentication Setup](AUTHENTICATION_SETUP.md) - Google OAuth configuration
- [Swagger UI](https://localhost:5001/swagger) - Interactive API documentation

## Support

For issues or questions:
1. Check the application logs: `dotnet run` output
2. Review the troubleshooting section above
3. Test endpoints directly with curl before using MCP clients
4. Verify JWT token validity with `/api/auth/me`

---

Last updated: January 4, 2026
