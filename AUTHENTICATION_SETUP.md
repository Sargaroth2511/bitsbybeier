# Google OAuth 2.0 Configuration

## Backend Configuration

Set the following environment variables before running the application:

```bash
# Google OAuth Client Credentials
export GOOGLE_CLIENT_ID="your-google-client-id.apps.googleusercontent.com"
export GOOGLE_CLIENT_SECRET="your-google-client-secret"

# JWT Secret (use a strong random string, minimum 32 characters)
export JWT_SECRET="your-jwt-secret-key-minimum-32-characters-long"
```

Or configure them in your appsettings.Development.json:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-google-client-secret"
    },
    "Jwt": {
      "Secret": "your-jwt-secret-key-minimum-32-characters-long"
    }
  }
}
```

## Frontend Configuration

Update the Google Client ID in `/ClientApp/src/app/login/login.component.ts`:

Replace `'YOUR_GOOGLE_CLIENT_ID'` with your actual Google Client ID on line 65.

Alternatively, you can create an environment file at `/ClientApp/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  googleClientId: 'your-google-client-id.apps.googleusercontent.com'
};
```

## How to Obtain Google OAuth Credentials

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Navigate to "APIs & Services" > "Credentials"
4. Click "Create Credentials" > "OAuth client ID"
5. Configure the OAuth consent screen if you haven't already
6. Select "Web application" as the application type
7. Add authorized redirect URIs:
   - `http://localhost:4200` (for development)
   - `https://localhost:4200` (for development with HTTPS)
   - Your production domain
8. Copy the Client ID and Client Secret

## CORS Configuration

The backend is configured to allow requests from:
- `http://localhost:4200`
- `https://localhost:4200`

To add additional origins for production, update the `Cors:AllowedOrigins` array in `appsettings.json`.

## Security Notes

- Never commit actual credentials to version control
- Use environment variables or secure secret management for production
- Ensure HTTPS is enabled in production
- The default JWT secret in the code is for development only
- Consider implementing refresh tokens for production use
- Consider restricting authentication to specific email domains if needed
