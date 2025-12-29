# Swagger UI Testing Guide

This guide shows you how to test the authenticated API endpoints using Swagger UI.

## Accessing Swagger UI

In development mode, navigate to:
```
http://localhost:5000/swagger
```
or
```
https://localhost:5001/swagger
```

## Testing Authenticated Endpoints

### Step 1: Obtain a JWT Token

First, you need to authenticate and get a JWT token. You have two options:

#### Option A: Use Google Authentication (Recommended)
1. Use the Angular frontend to sign in with Google
2. The JWT token will be stored in the browser
3. Copy the token from browser storage or network requests

#### Option B: Manual Authentication (if frontend not available)
You can use tools like Postman or curl to authenticate:
```bash
curl -X POST https://localhost:5001/api/auth/google \
  -H "Content-Type: application/json" \
  -d '{"idToken": "YOUR_GOOGLE_ID_TOKEN"}'
```

The response will contain a JWT token:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {...}
}
```

### Step 2: Configure Swagger Authentication

1. **Open Swagger UI** at `/swagger`

2. **Click the "Authorize" button** (green lock icon in the top right)

3. **Enter your JWT token** in the "Value" field using this format:
   ```
   Bearer YOUR_TOKEN_HERE
   ```
   
   Example:
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
   ```

4. **Click "Authorize"**

5. **Click "Close"**

### Step 3: Test API Endpoints

Now you can test any endpoint that requires authentication:

#### Testing Content Creation (POST /api/cms/content)

1. Expand the `POST /api/cms/content` endpoint
2. Click "Try it out"
3. Enter the request body:
   ```json
   {
     "author": "John Doe",
     "title": "My First Article",
     "subtitle": "A guide to getting started",
     "content": "# Welcome\n\nThis is my first article with **Markdown** support!",
     "draft": true
   }
   ```
4. Click "Execute"
5. View the response below

The request will include your JWT token in the Authorization header automatically.

#### Testing MCP Info Endpoint (GET /api/mcp/info)

1. Expand the `GET /api/mcp/info` endpoint
2. Click "Try it out"
3. Click "Execute"
4. View the MCP server information and available tools

**Note**: This endpoint requires Admin role. Make sure your user has the Admin role assigned.

## Troubleshooting

### 401 Unauthorized
- Your token may have expired
- Obtain a new token and update it in Swagger
- Make sure you entered only the token (no "Bearer " prefix - Swagger adds it)

### 403 Forbidden
- Your user doesn't have the required role (Admin)
- Check that your user has Admin role in the database
- MCP and CMS endpoints require Admin role

### Token Format Error
- Enter only the raw JWT token
- Don't include "Bearer " prefix (Swagger adds it automatically)
- Don't include quotes around the token

## Swagger Features

### Authorization Badge
- Once authorized, you'll see a lock icon next to endpoints
- Locked icon = requires authentication
- All requests will automatically include your token

### Response Codes
- **200/201**: Success
- **400**: Bad Request (validation error)
- **401**: Unauthorized (not authenticated)
- **403**: Forbidden (not authorized/wrong role)

### Validation Errors
The API automatically validates:
- **Author**: Required, max 200 characters
- **Title**: Required, max 500 characters
- **Subtitle**: Optional, max 1000 characters
- **Content**: Required, no length limit
- **Draft**: Optional, boolean (default: true)

## Example Workflow

1. Authenticate via Google (using frontend or API)
2. Copy JWT token from response
3. Open Swagger UI at `/swagger`
4. Click "Authorize" button
5. Enter: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...` (just the token, no "Bearer")
6. Click "Authorize" and "Close"
7. Test any endpoint with "Try it out"
8. View request/response details

## Security Notes

- Tokens expire after a configured time (check JWT settings)
- Only Admin users can access CMS and MCP endpoints
- Always use HTTPS in production
- Never share your JWT tokens
- Tokens are session-specific

## Additional Resources

- JWT Documentation: https://jwt.io/
- Swagger UI Guide: https://swagger.io/tools/swagger-ui/
- API Authentication: See `MCP_CONTENT_API_README.md`
