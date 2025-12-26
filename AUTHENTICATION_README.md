# Google OAuth 2.0 Authentication Implementation

This document describes the Google OAuth 2.0 authentication system implemented for the bitsbybeier CMS.

## Overview

The authentication system uses Google OAuth 2.0 for user authentication and JWT (JSON Web Tokens) for API authorization. Users sign in with their Google account on the frontend, the backend validates the Google token and issues a JWT, which is then used to authorize subsequent API requests.

## Architecture

### Authentication Flow

1. User clicks "Sign in with Google" on the `/login` page
2. Google Sign-In button (via Google Identity Services) initiates OAuth flow
3. User authenticates with Google
4. Google returns an ID token to the frontend
5. Frontend sends the Google ID token to the backend `/api/auth/google` endpoint
6. Backend validates the Google ID token using Google.Apis.Auth library
7. Backend generates a JWT token containing user claims (email, name)
8. Frontend stores the JWT in localStorage
9. Frontend includes the JWT in the Authorization header for all subsequent API requests
10. Backend validates JWT on protected endpoints (e.g., `/api/cms`)

### Backend Components

#### Controllers

- **AuthController** (`/api/auth`)
  - `POST /api/auth/google` - Validates Google ID token and issues JWT
  - `GET /api/auth/user` - Returns current user information (protected)

- **CmsController** (`/api/cms`)
  - `GET /api/cms` - CMS welcome endpoint (protected)
  - `GET /api/cms/content` - Get content items (protected)
  - `POST /api/cms/content` - Create content item (protected)

#### Services

- **JwtTokenService** - Generates JWT tokens with user claims
  - Configurable issuer, audience, and expiration
  - Uses symmetric key signing (HMAC-SHA256)

#### Configuration (`Program.cs`)

- JWT Bearer authentication as default scheme
- Google OAuth configuration
- CORS policy for frontend origins
- Authorization policies

### Frontend Components

#### Services

- **AuthService** (`auth.service.ts`)
  - Manages user authentication state
  - Stores/retrieves JWT from localStorage
  - Provides authentication methods
  - Exposes current user as Observable

#### Guards

- **AuthGuard** - Protects routes requiring authentication
  - Redirects unauthenticated users to `/login`
  - Preserves return URL for post-login navigation

#### Interceptors

- **JwtInterceptor** - Automatically adds JWT to HTTP requests
  - Attaches `Authorization: Bearer <token>` header
  - Applied to all outgoing HTTP requests

#### Components

- **LoginComponent** (`/login`)
  - Google Sign-In button
  - Handles OAuth callback
  - Redirects to return URL after successful authentication

- **CmsComponent** (`/cms`)
  - Protected CMS interface
  - Displays user information
  - Shows sample content from API

## Configuration

### Backend Configuration

1. **appsettings.json** - Configure authentication settings:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "${GOOGLE_CLIENT_ID}",
      "ClientSecret": "${GOOGLE_CLIENT_SECRET}"
    },
    "Jwt": {
      "Secret": "${JWT_SECRET}",
      "Issuer": "bitsbybeier",
      "Audience": "bitsbybeier-app",
      "ExpirationMinutes": 60
    }
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://localhost:4200"
    ]
  }
}
```

2. **Environment Variables** - Set the following:
   - `GOOGLE_CLIENT_ID` - Your Google OAuth Client ID
   - `GOOGLE_CLIENT_SECRET` - Your Google OAuth Client Secret
   - `JWT_SECRET` - Strong random key (minimum 32 characters)

### Frontend Configuration

1. **environments/environment.ts** - Configure Google Client ID:

```typescript
export const environment = {
  production: false,
  googleClientId: 'YOUR_GOOGLE_CLIENT_ID'
};
```

## Security Considerations

### Implemented Security Measures

1. **HTTPS Required** - `UseHttpsRedirection()` enforces HTTPS in production
2. **CORS Policy** - Restricts API access to configured origins
3. **JWT Validation** - All protected endpoints validate JWT signature, issuer, audience, and expiration
4. **Google Token Validation** - Backend validates Google ID tokens before issuing JWT
5. **Minimal Token Claims** - JWT only contains necessary user information

### Additional Recommendations

1. **Token Refresh** - Consider implementing refresh tokens for long-lived sessions
2. **Email Domain Restriction** - Optionally restrict access to specific email domains
3. **Rate Limiting** - Add rate limiting to authentication endpoints
4. **Logging** - Add authentication event logging for audit purposes
5. **Secret Management** - Use Azure Key Vault or similar for production secrets

## API Endpoints

### Public Endpoints

- `POST /api/auth/google` - Exchange Google ID token for JWT
  - Request: `{ "idToken": "..." }`
  - Response: `{ "token": "...", "email": "...", "name": "..." }`

### Protected Endpoints (Require JWT)

- `GET /api/auth/user` - Get current user information
- `GET /api/cms` - CMS welcome message
- `GET /api/cms/content` - List content items
- `POST /api/cms/content` - Create content item

All protected endpoints require the `Authorization: Bearer <token>` header.

## Development Setup

See [AUTHENTICATION_SETUP.md](AUTHENTICATION_SETUP.md) for detailed setup instructions.

## Testing

### Manual Testing

1. Start the backend: `dotnet run`
2. Start the frontend: `cd ClientApp && npm start`
3. Navigate to `http://localhost:4200`
4. Click "Login" and sign in with Google
5. Access the CMS at `http://localhost:4200/cms`
6. Verify API requests include JWT token

### Testing Protected Endpoints

Use tools like Postman or curl:

```bash
# Get JWT token first via login flow, then:
curl -H "Authorization: Bearer <your-jwt-token>" http://localhost:5000/api/cms/content
```

## Troubleshooting

### Common Issues

1. **Google Sign-In button not appearing**
   - Check Google Client ID is configured in environment.ts
   - Verify Google GSI script loads successfully
   - Check browser console for errors

2. **401 Unauthorized on API requests**
   - Verify JWT is present in localStorage
   - Check JWT hasn't expired
   - Ensure Authorization header is being sent

3. **CORS errors**
   - Verify frontend origin is in Cors:AllowedOrigins
   - Check CORS middleware is configured correctly

## Future Enhancements

- Implement refresh tokens
- Add role-based authorization
- Implement email domain restrictions
- Add multi-factor authentication
- Enhance audit logging
- Add user profile management
