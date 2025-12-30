# Swagger UI Testing Guide

This guide shows you how to test the authenticated API endpoints using Swagger UI with **two authentication options**: OAuth2 (simpler) or Bearer Token (manual).

## Accessing Swagger UI

In development mode, navigate to:
```
http://localhost:5000/swagger
```
or
```
https://localhost:5001/swagger
```

## Authentication Options

You can authenticate using either method:

### Option 1: OAuth2 Google Login (Recommended - Simpler!)

This is the easiest method - click a button and login with Google.

1. **Open Swagger UI** at `/swagger`

2. **Click the "Authorize" button** (lock icon in the top right)

3. **You'll see two authentication options**:
   - **oauth2** (Google OAuth2) - Recommended
   - **bearer** (Manual JWT token)

4. **Click "Authorize" under oauth2** section

5. **Login with Google**:
   - You'll be redirected to Google's login page
   - Sign in with your Google account
   - Grant permissions when prompted (openid, profile, email)
   
6. **Automatically authenticated**:
   - After successful login, you'll be redirected back to Swagger
   - Google provides an ID token
   - Use `/api/auth/google` endpoint to exchange it for JWT if needed
   - All subsequent requests can use the authentication

7. **Click "Close"** to return to Swagger UI

8. **Test endpoints** - lock icons will appear on protected routes

**Note**: For OAuth2 to work properly:
- Make sure your Google OAuth credentials are configured
- The redirect URI should include: `https://localhost:5001/swagger/oauth2-redirect.html`
- Add this to your Google Cloud Console's Authorized redirect URIs

### Option 2: Bearer Token (Manual)

This method requires you to obtain a JWT token manually first.

#### Step 2a: Obtain a JWT Token

You can get a JWT token by:

**Using the API directly:**
```bash
curl -X POST https://localhost:5001/api/auth/google \
  -H "Content-Type: application/json" \
  -d '{"idToken": "YOUR_GOOGLE_ID_TOKEN"}'
```

The response will contain a JWT token:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "user@example.com",
  "name": "User Name",
  "role": "Admin",
  "userId": 1
}
```

#### Step 2b: Configure Bearer Authentication

1. **Open Swagger UI** at `/swagger`

2. **Click the "Authorize" button** (lock icon in the top right)

3. **Select Bearer authentication** from the available options

4. **Enter your JWT token** in the "Value" field:
   - **Important**: Enter ONLY the JWT token, NOT "Bearer YOUR_TOKEN"
   - Swagger automatically adds the "Bearer " prefix
   
   Example - Enter this:
   ```
   eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

5. **Click "Authorize"** under the Bearer section

6. **Click "Close"**

7. **Test endpoints** - lock icons will appear on protected routes

## Testing API Endpoints

Once authenticated (using either method), you can test endpoints:

### Testing Content Creation (POST /api/cms/content)

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

The request will include your authentication automatically.

### Testing MCP Info Endpoint (GET /api/mcp/info)

1. Expand the `GET /api/mcp/info` endpoint
2. Click "Try it out"
3. Click "Execute"
4. View the MCP server information and available tools

**Note**: This endpoint requires Admin role. Make sure your user has the Admin role assigned.

## Troubleshooting

### OAuth2 Issues

#### Redirect URI Mismatch
- Error: "redirect_uri_mismatch"
- Solution: Add `https://localhost:5001/swagger/oauth2-redirect.html` to Google Cloud Console > APIs & Services > Credentials > OAuth 2.0 Client IDs > Authorized redirect URIs

#### OAuth2 Not Working
- Make sure Google ClientId is configured in appsettings.json
- Check browser console for errors
- Verify you're using HTTPS (required by Google OAuth)

### Bearer Token Issues

#### 401 Unauthorized
- Your token may have expired
- Obtain a new token and update it in Swagger
- Make sure you entered only the token (no "Bearer " prefix - Swagger adds it)

#### 403 Forbidden
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
- All requests will automatically include your authentication

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

## Comparison: OAuth2 vs Bearer Token

| Feature | OAuth2 (Recommended) | Bearer Token |
|---------|---------------------|--------------|
| **Ease of Use** | ⭐⭐⭐⭐⭐ Click and login | ⭐⭐⭐ Manual token copy |
| **Setup** | Requires Google OAuth setup | No additional setup |
| **Security** | ⭐⭐⭐⭐⭐ Industry standard | ⭐⭐⭐⭐ Secure if token protected |
| **Token Management** | Automatic | Manual renewal |
| **Best For** | Interactive testing | Automated testing, CI/CD |

## Security Notes

- Tokens expire after a configured time (check JWT settings)
- Only Admin users can access CMS and MCP endpoints
- Always use HTTPS in production
- Never share your JWT tokens
- Tokens are session-specific
- OAuth2 tokens are automatically managed

## Additional Resources

- JWT Documentation: https://jwt.io/
- Swagger UI Guide: https://swagger.io/tools/swagger-ui/
- Google OAuth2: https://developers.google.com/identity/protocols/oauth2
- API Authentication: See `MCP_CONTENT_API_README.md`
