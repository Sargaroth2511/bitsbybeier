# MCP Server and Content API

This document describes the Model Context Protocol (MCP) server and Content API implementation for creating content programmatically.

## Overview

The implementation provides:
- **MCP Server**: Exposes tools for AI agents to create content via the Model Context Protocol
- **REST API**: Provides HTTP endpoints for content creation accessible to any client
- **Database**: Stores content with support for drafts, images, and Markdown formatting

## Authentication & Authorization

### Security Model

Both the MCP server and REST API require authentication to ensure secure access:

- **MCP Endpoints**: Require authentication (JWT bearer token)
- **REST API Endpoints**: Require authentication and Admin role authorization
- **Standard**: Uses OAuth 2.1 compatible JWT tokens

This follows the standard MCP authentication pattern where AI agents authenticate using OAuth flows before accessing tools and resources.

### How to Authenticate

1. Obtain a JWT token via the `/api/auth/google` endpoint
2. Include the token in the Authorization header: `Authorization: Bearer <token>`
3. For MCP clients, configure the token in the transport options

## Content Creation

### Via REST API

**Endpoint**: `POST /api/cms/content`  
**Authorization**: Requires Admin role  
**Content-Type**: `application/json`

#### Request Body

```json
{
  "author": "John Doe",
  "title": "My Article Title",
  "subtitle": "Optional subtitle or summary",
  "content": "Main content text with **Markdown** support",
  "draft": true
}
```

#### Response (201 Created)

```json
{
  "id": 1,
  "author": "John Doe",
  "title": "My Article Title",
  "subtitle": "Optional subtitle or summary",
  "content": "Main content text with **Markdown** support",
  "draft": true,
  "active": true,
  "createdAt": "2025-12-28T19:30:00Z",
  "updatedAt": null
}
```

### Via MCP Server

The MCP server exposes a `CreateContentAsync` tool that AI agents can invoke programmatically.

#### Tool: CreateContentAsync

**Description**: Creates a new content item in the CMS. The content is created as a draft by default and will not be published immediately.

**Parameters**:
- `request` (object, required): Content creation request containing:
  - `author` (string, required): The author name of the content
  - `title` (string, required): The title of the content
  - `content` (string, required): The main content text. Supports Markdown formatting
  - `subtitle` (string, optional): Optional subtitle or summary of the content
  - `draft` (boolean, optional, default: true): Whether to create as draft

**Returns**: A message indicating the content was created successfully with details including ID, title, draft status, and creation timestamp.

#### MCP Server Information

**Endpoint**: `GET /api/mcp/info`  
**Authorization**: Requires authentication

Returns information about the MCP server and available tools.

## Database Schema

### Content Table

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key, auto-generated |
| Author | string(200) | Author name (required) |
| Title | string(500) | Content title (required) |
| Subtitle | string(1000) | Optional subtitle |
| ContentText | text | Main content, supports Markdown (required) |
| Draft | boolean | Whether content is in draft state (default: true) |
| Active | boolean | Whether content is active (default: true) |
| CreatedAt | datetime | UTC timestamp when created (auto-set) |
| UpdatedAt | datetime | UTC timestamp when last updated (nullable) |

### ContentImage Table

| Field | Type | Description |
|-------|------|-------------|
| Id | int | Primary key, auto-generated |
| ImageData | bytea | Binary image data (required) |
| ContentType | string(100) | MIME type (e.g., "image/jpeg") |
| FileName | string(255) | Original filename (optional) |
| FileSize | long | File size in bytes |
| UploadedAt | datetime | UTC timestamp when uploaded (auto-set) |
| ContentId | int | Foreign key to Content table |

Images are linked to content items via the `ContentId` foreign key with cascade delete.

## Draft Behavior

Content created with `draft: true` (the default):
- Is stored in the database
- Can be retrieved via API endpoints (with proper authorization)
- Is NOT automatically published or visible to end users
- Can be used for preview purposes
- Must be explicitly published through a separate workflow (not implemented in this scope)

## Example Usage

### cURL Example

```bash
# Create content via API
curl -X POST https://your-domain/api/cms/content \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "author": "Jane Smith",
    "title": "Getting Started with MCP",
    "subtitle": "A beginner-friendly guide",
    "content": "# Introduction\n\nThis guide will help you get started...",
    "draft": true
  }'
```

### MCP Client Example (C#)

```csharp
using ModelContextProtocol.Client;

// Connect to MCP server with authentication
var client = await McpClient.CreateAsync(transport);

// List available tools (requires authentication)
var tools = await client.ListToolsAsync();

// Invoke content creation tool with ContentRequest model
var result = await client.CallToolAsync("CreateContentAsync", new Dictionary<string, object?>
{
    ["request"] = new 
    {
        author = "Jane Smith",
        title = "Getting Started with MCP",
        content = "# Introduction\n\nThis guide will help you...",
        subtitle = "A beginner-friendly guide",
        draft = true
    }
});

Console.WriteLine(result.Content.First().Text);
// Output: Content created successfully. ID: 1, Title: Getting Started with MCP, Draft: True, Created: 2025-12-28 19:30:00 UTC
```

## Architecture

### Base Controller Pattern

All API controllers inherit from `BaseController` which provides common functionality:
- **Logger**: Centralized logging available via `Logger` property
- **User Context**: Access to authenticated user information
  - `UserId`: Current user's ID from claims
  - `UserEmail`: Current user's email from claims
  - `UserDisplayName`: Current user's display name from claims
  - `IsAuthenticated`: Check if user is authenticated
- **Consistent Design**: Ensures all controllers follow the same patterns

This pattern promotes code reuse and consistency across all controllers.

### Service Layer

The `ContentService` uses a model-based approach:
- Accepts `ContentRequest` objects instead of multiple parameters
- Promotes cleaner API design and easier maintenance
- Reduces parameter coupling and improves testability
- Follows best practices for service layer patterns

## Security

- The REST API requires authentication and Admin role authorization
- The MCP API requires authentication for all endpoints (follows OAuth 2.1 standards)
- All inputs are validated before processing
- SQL injection is prevented through Entity Framework parameterized queries
- Content is sanitized at the application layer (HTML/Markdown rendering should use appropriate sanitizers)
- No vulnerabilities found in dependencies (checked via GitHub Advisory Database)
- Follows standard MCP authentication patterns used by ChatGPT and other AI agents

## Implementation Details

- **Framework**: ASP.NET Core (.NET 9.0)
- **ORM**: Entity Framework Core 9.0
- **Database**: PostgreSQL
- **MCP SDK**: ModelContextProtocol v0.5.0-preview.1
- **Pattern**: Code-First with EF Migrations
- **Architecture**: Base controller pattern with model-based service layer

## Future Enhancements (Out of Scope)

The following features are explicitly out of scope for this implementation:
- Content editing or deletion
- Publishing workflows and approval processes
- Content versioning
- Frontend implementation
- Image upload endpoints (table exists but no upload API)
