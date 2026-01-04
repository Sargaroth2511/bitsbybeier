# OAuth 2.0 Setup Guide for ChatGPT Integration

This guide explains how to set up and use OAuth 2.0 authentication to connect ChatGPT custom apps to the BitsbyBeier MCP server.

## Overview

The BitsbyBeier MCP server now supports OAuth 2.0 Authorization Code flow with PKCE (Proof Key for Code Exchange), enabling secure integration with ChatGPT custom apps and other OAuth-compatible clients.

## Architecture

```
User → ChatGPT → OAuth Flow → BitsbyBeier Server → MCP Tools
         ↓
    Authorization
    Access Token
    Refresh Token
```

## Prerequisites

1. BitsbyBeier application running (locally or deployed)
2. PostgreSQL database with OAuth tables migrated
3. Admin user account created
4. Google OAuth configured (for user authentication)

## Step 1: Database Setup

Ensure the OAuth tables are created by running migrations:

```bash
cd /home/runner/work/bitsbybeier/bitsbybeier
dotnet ef database update
```

This creates three tables:
- `OAuthClients` - Registered OAuth client applications
- `OAuthAuthorizationCodes` - Short-lived authorization codes
- `OAuthRefreshTokens` - Long-lived refresh tokens

## Step 2: Create an OAuth Client

As an admin user, create an OAuth client for your ChatGPT app:

### Using API

```bash
# First, get your admin JWT token by logging in
curl -k -X POST https://localhost:5001/api/auth/google \
  -H "Content-Type: application/json" \
  -d '{"idToken": "YOUR_GOOGLE_ID_TOKEN"}'

# Save the JWT token from the response

# Create OAuth client
curl -k -X POST https://localhost:5001/api/oauth/admin/clients \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "clientName": "ChatGPT MCP Integration",
    "redirectUris": [
      "https://chat.openai.com/aip/oauth/callback",
      "http://localhost:3000/callback"
    ],
    "allowedScopes": ["mcp:read", "mcp:write"]
  }'
```

### Response

```json
{
  "clientId": "abc123def456...",
  "clientSecret": "xyz789uvw012...",
  "clientName": "ChatGPT MCP Integration",
  "redirectUris": ["https://chat.openai.com/aip/oauth/callback", ...],
  "allowedScopes": ["mcp:read", "mcp:write"],
  "message": "Save the client secret securely. It cannot be retrieved later."
}
```

**⚠️ IMPORTANT:** Save both `clientId` and `clientSecret` immediately. The client secret is shown only once and cannot be retrieved later!

## Step 3: OAuth Endpoints

Your BitsbyBeier server now exposes these OAuth 2.0 endpoints:

### Authorization Endpoint

**URL:** `https://your-domain.com/oauth/authorize`

**Method:** GET

**Query Parameters:**
- `response_type` (required): Must be "code"
- `client_id` (required): Your OAuth client ID
- `redirect_uri` (required): One of your registered redirect URIs
- `scope` (optional): Space-separated scopes (default: "mcp:read mcp:write")
- `state` (optional): Random string for CSRF protection
- `code_challenge` (optional): PKCE code challenge
- `code_challenge_method` (optional): "S256" or "plain"

**Example:**
```
https://your-domain.com/oauth/authorize?
  response_type=code&
  client_id=abc123def456&
  redirect_uri=https://chat.openai.com/aip/oauth/callback&
  scope=mcp:read%20mcp:write&
  state=random_state_string&
  code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM&
  code_challenge_method=S256
```

### Token Endpoint

**URL:** `https://your-domain.com/oauth/token`

**Method:** POST

**Content-Type:** `application/x-www-form-urlencoded`

**Parameters for Authorization Code Grant:**
- `grant_type`: "authorization_code"
- `code`: Authorization code from /authorize
- `redirect_uri`: Same redirect URI used in /authorize
- `client_id`: Your OAuth client ID
- `client_secret`: Your OAuth client secret
- `code_verifier` (optional): PKCE code verifier

**Example:**
```bash
curl -X POST https://your-domain.com/oauth/token \
  -d "grant_type=authorization_code" \
  -d "code=AUTH_CODE" \
  -d "redirect_uri=https://chat.openai.com/aip/oauth/callback" \
  -d "client_id=abc123def456" \
  -d "client_secret=xyz789uvw012"
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "refresh_token": "refresh_token_here",
  "scope": "mcp:read mcp:write"
}
```

**Parameters for Refresh Token Grant:**
- `grant_type`: "refresh_token"
- `refresh_token`: Your refresh token
- `client_id`: Your OAuth client ID
- `client_secret`: Your OAuth client secret

### Token Revocation Endpoint

**URL:** `https://your-domain.com/oauth/revoke`

**Method:** POST

**Parameters:**
- `token`: The refresh token to revoke
- `client_id` (optional): Your OAuth client ID
- `client_secret` (optional): Your OAuth client secret

## Step 4: Configure ChatGPT Custom App

In ChatGPT's custom app settings:

1. **Authorization URL:** `https://your-domain.com/oauth/authorize`
2. **Token URL:** `https://your-domain.com/oauth/token`
3. **Client ID:** Your OAuth client ID from Step 2
4. **Client Secret:** Your OAuth client secret from Step 2
5. **Scope:** `mcp:read mcp:write`
6. **MCP Server URL:** `https://your-domain.com/api/mcp`

## Step 5: Test the OAuth Flow

### Manual Testing

```bash
# 1. Get authorization code (requires browser for user login)
# Open in browser:
https://localhost:5001/oauth/authorize?response_type=code&client_id=YOUR_CLIENT_ID&redirect_uri=http://localhost:3000/callback&scope=mcp:read%20mcp:write

# After login and approval, you'll be redirected to:
# http://localhost:3000/callback?code=AUTHORIZATION_CODE

# 2. Exchange code for tokens
curl -k -X POST https://localhost:5001/oauth/token \
  -d "grant_type=authorization_code" \
  -d "code=AUTHORIZATION_CODE" \
  -d "redirect_uri=http://localhost:3000/callback" \
  -d "client_id=YOUR_CLIENT_ID" \
  -d "client_secret=YOUR_CLIENT_SECRET"

# 3. Use access token to call MCP endpoints
curl -k https://localhost:5001/api/mcp/info \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"

# 4. Refresh the access token when it expires
curl -k -X POST https://localhost:5001/oauth/token \
  -d "grant_type=refresh_token" \
  -d "refresh_token=YOUR_REFRESH_TOKEN" \
  -d "client_id=YOUR_CLIENT_ID" \
  -d "client_secret=YOUR_CLIENT_SECRET"
```

## Management Operations

### List All OAuth Clients

```bash
curl -k https://localhost:5001/api/oauth/admin/clients \
  -H "Authorization: Bearer YOUR_ADMIN_JWT_TOKEN"
```

### Deactivate an OAuth Client

```bash
curl -k -X DELETE https://localhost:5001/api/oauth/admin/clients/YOUR_CLIENT_ID \
  -H "Authorization: Bearer YOUR_ADMIN_JWT_TOKEN"
```

## Security Considerations

### For Development

✅ **Acceptable for local development:**
- Using `localhost` redirect URIs
- Self-signed SSL certificates (with `-k` flag in curl)
- Using `NODE_TLS_REJECT_UNAUTHORIZED=0` environment variable

### For Production

⚠️ **Required for production:**

1. **HTTPS Only:** Use valid SSL/TLS certificates (Let's Encrypt, etc.)
2. **Secure Redirect URIs:** Register only HTTPS redirect URIs
3. **Environment Variables:** Store client secrets in secure vaults (Azure Key Vault, AWS Secrets Manager)
4. **Rate Limiting:** Implement rate limiting on OAuth endpoints
5. **Token Cleanup:** Schedule jobs to delete expired tokens
6. **Audit Logging:** Monitor all OAuth operations
7. **CORS Configuration:** Configure proper CORS policies for production domains

### Best Practices

1. **Never commit secrets** to version control
2. **Rotate client secrets** regularly
3. **Use PKCE** for all clients (especially public clients)
4. **Keep authorization codes short-lived** (10 minutes default)
5. **Monitor failed authentication attempts**
6. **Implement refresh token rotation** for enhanced security
7. **Validate all redirect URIs** strictly

## Scopes

The following scopes are available:

- `mcp:read` - Read access to MCP resources (required for listing tools)
- `mcp:write` - Write access to MCP resources (required for executing tools)

Default scopes: `mcp:read mcp:write`

## Token Lifetimes

- **Authorization Codes:** 10 minutes
- **Access Tokens:** 60 minutes (configurable in appsettings.json)
- **Refresh Tokens:** 30 days

## Troubleshooting

### "invalid_client" Error

- Verify `client_id` and `client_secret` are correct
- Ensure the client is active (not deactivated)
- Check that client secret hasn't been rotated

### "invalid_redirect_uri" Error

- Confirm the redirect URI matches exactly (including protocol and trailing slashes)
- Verify the URI is registered in the client's `redirectUris` list

### "invalid_scope" Error

- Check that requested scopes are in the client's `allowedScopes` list
- Use space-separated scope format: `mcp:read mcp:write`

### "invalid_grant" Error

- Authorization code may be expired (10 minute lifetime)
- Code may have already been used (one-time use only)
- Refresh token may be expired or revoked
- PKCE verifier may not match the challenge

### "unsupported_grant_type" Error

- Only "authorization_code" and "refresh_token" grant types are supported
- Check the `grant_type` parameter value

## PKCE (Proof Key for Code Exchange)

PKCE is recommended for all OAuth clients and required for public clients.

### How to Use PKCE

1. **Generate code verifier:**
   ```javascript
   const verifier = crypto.randomBytes(32).toString('base64url');
   ```

2. **Generate code challenge:**
   ```javascript
   const challenge = crypto.createHash('sha256')
     .update(verifier)
     .digest('base64url');
   ```

3. **Include in authorization request:**
   ```
   /oauth/authorize?...&code_challenge=CHALLENGE&code_challenge_method=S256
   ```

4. **Include verifier in token request:**
   ```
   /oauth/token?...&code_verifier=VERIFIER
   ```

## Further Reading

- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [OAuth 2.0 Security Best Practices](https://tools.ietf.org/html/rfc8252)
- [MCP Content API README](MCP_CONTENT_API_README.md)

---

Last updated: January 4, 2026
