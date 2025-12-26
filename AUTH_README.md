# Google OAuth Authentication Implementation

This implementation adds Google OAuth 2.0 authentication to the bitsbybeier application with JWT token-based authorization for the CMS API endpoints.

## Architecture

### Backend (ASP.NET Core 9.0)
- **Google OAuth**: Handles authentication via Google's OAuth 2.0 provider
- **JWT Bearer**: Issues and validates JWT tokens for API authorization
- **Protected Endpoints**: CMS API endpoints require valid JWT tokens

### Frontend (Angular 21)
- **Authentication Service**: Manages OAuth flow and token storage
- **HTTP Interceptor**: Automatically attaches JWT tokens to API requests
- **Route Guards**: Protects CMS pages from unauthorized access
- **Auth Callback**: Handles OAuth redirect and token storage

## Configuration

### Prerequisites
1. Create a Google OAuth 2.0 Client ID:
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create or select a project
   - Enable Google+ API
   - Create OAuth 2.0 credentials
   - Add authorized redirect URIs:
     - `https://localhost:7140/api/auth/google-callback`
     - `https://localhost:7140/signin-google`

2. Generate a JWT secret key (at least 32 characters)

### Backend Configuration

Update `appsettings.json` or use environment variables:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Jwt": {
      "SecretKey": "YOUR_SECURE_JWT_SECRET_KEY_AT_LEAST_32_CHARACTERS",
      "Issuer": "https://localhost:7140",
      "Audience": "https://localhost:4200",
      "ExpirationMinutes": 60
    }
  }
}
```

For production, use environment variables:
- `Authentication__Google__ClientId`
- `Authentication__Google__ClientSecret`
- `Authentication__Jwt__SecretKey`

### Frontend Configuration

The frontend is configured to work with the backend at:
- Backend API: `https://localhost:7140`
- Frontend: `http://localhost:4200`

Update URLs in the following files if needed:
- `ClientApp/src/app/services/auth.service.ts`
- `ClientApp/src/app/cms/cms.component.ts`

## Usage

### Starting the Application

1. **Backend**:
   ```bash
   dotnet run
   ```

2. **Frontend**:
   ```bash
   cd ClientApp
   npm start
   ```

### Authentication Flow

1. User clicks "Login with Google" on the home page
2. User is redirected to Google OAuth consent screen
3. After successful authentication, Google redirects to `/api/auth/google-callback`
4. Backend validates Google token and generates JWT token
5. User is redirected to Angular app at `/auth-callback?token=JWT_TOKEN`
6. Angular app stores token and redirects to `/cms`
7. All API requests to protected endpoints include JWT token in Authorization header

### API Endpoints

#### Public Endpoints
- `GET /api/health` - Health check
- `GET /api/auth/login` - Initiates Google OAuth flow
- `GET /api/auth/google-callback` - OAuth callback (internal)
- `POST /api/auth/validate-token` - Validates JWT token

#### Protected Endpoints (Require JWT)
- `GET /api/cms/content` - Get CMS content
- `POST /api/cms/content` - Create CMS content
- `PUT /api/cms/content/{id}` - Update CMS content
- `DELETE /api/cms/content/{id}` - Delete CMS content

## Security Considerations

1. **HTTPS Required**: All authentication flows require HTTPS in production
2. **CORS Configuration**: Configured for localhost development, update for production
3. **JWT Secret**: Must be at least 32 characters and stored securely
4. **Token Expiration**: Default 60 minutes, configurable
5. **Google Credentials**: Never commit client secrets to version control
6. **Domain Restriction**: Consider restricting to specific email domains in production

## Development Notes

- The `oidc-client` package is included in dependencies but not currently used
- Angular components use standalone: false for compatibility with existing module structure
- Bootstrap 5 is used for styling
- JWT tokens are stored in localStorage (consider using httpOnly cookies for production)

## Troubleshooting

1. **CORS Errors**: Ensure CORS policy includes your frontend URL
2. **401 Unauthorized**: Check JWT token is present and valid
3. **Google OAuth Errors**: Verify redirect URIs match in Google Console
4. **Certificate Errors**: Use `dotnet dev-certs https --trust` for local HTTPS

## Future Enhancements

- Add email domain restriction for organization-only access
- Implement refresh tokens for extended sessions
- Add role-based authorization
- Store JWT in httpOnly cookies instead of localStorage
- Add user profile management
- Implement proper CMS data persistence (currently mock data)
