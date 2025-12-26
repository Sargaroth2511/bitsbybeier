# Quick Start Guide: Google OAuth 2.0 Authentication

This guide will help you get the authentication system up and running in under 10 minutes.

## Prerequisites

- Node.js 18+ and npm
- .NET 9.0 SDK
- Google Cloud account
- A web browser

## Step 1: Get Google OAuth Credentials (5 minutes)

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project (or select existing)
3. Navigate to **APIs & Services** → **Credentials**
4. Click **Create Credentials** → **OAuth client ID**
5. If prompted, configure the OAuth consent screen:
   - User Type: External
   - App name: bitsbybeier
   - Support email: Your email
   - Click Save and Continue through the screens
6. Back at Create OAuth client ID:
   - Application type: **Web application**
   - Name: bitsbybeier-dev
   - Authorized redirect URIs: `http://localhost:4200`
   - Click **Create**
7. Copy the **Client ID** and **Client Secret**

## Step 2: Configure Backend (2 minutes)

### Option A: Using appsettings.Development.json (Recommended for Development)

Edit `/appsettings.Development.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID_HERE.apps.googleusercontent.com",
      "ClientSecret": "YOUR_CLIENT_SECRET_HERE"
    },
    "Jwt": {
      "Secret": "development-jwt-secret-key-minimum-32-characters-long-do-not-use-in-production"
    }
  }
}
```

**Note**: The development JWT secret is already set. Just update the Google credentials.

### Option B: Using Environment Variables

```bash
# Linux/macOS
export GOOGLE_CLIENT_ID="your-client-id.apps.googleusercontent.com"
export GOOGLE_CLIENT_SECRET="your-client-secret"
export JWT_SECRET="your-strong-jwt-secret-minimum-32-characters"

# Windows PowerShell
$env:GOOGLE_CLIENT_ID="your-client-id.apps.googleusercontent.com"
$env:GOOGLE_CLIENT_SECRET="your-client-secret"
$env:JWT_SECRET="your-strong-jwt-secret-minimum-32-characters"

# Windows CMD
set GOOGLE_CLIENT_ID=your-client-id.apps.googleusercontent.com
set GOOGLE_CLIENT_SECRET=your-client-secret
set JWT_SECRET=your-strong-jwt-secret-minimum-32-characters
```

## Step 3: Configure Frontend (1 minute)

Edit `/ClientApp/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  googleClientId: 'YOUR_CLIENT_ID_HERE.apps.googleusercontent.com'
};
```

Replace `'YOUR_CLIENT_ID_HERE.apps.googleusercontent.com'` with your actual Google Client ID.

## Step 4: Run the Application (2 minutes)

### Terminal 1 - Backend
```bash
cd /path/to/bitsbybeier
dotnet run
```

The backend will start on `http://localhost:5000` (or similar).

### Terminal 2 - Frontend
```bash
cd /path/to/bitsbybeier/ClientApp
npm install  # First time only
npm start
```

The frontend will start on `http://localhost:4200`.

## Step 5: Test Authentication

1. Open browser to `http://localhost:4200`
2. Click **Login** in the navigation bar
3. Click the **Sign in with Google** button
4. Sign in with your Google account
5. You'll be redirected back to the home page
6. Your name should appear in the navigation bar
7. Click **CMS** to access the protected CMS page
8. You should see the CMS dashboard with sample content

## Troubleshooting

### Google Sign-In button doesn't appear
- Check browser console for errors
- Verify Google Client ID is correct in `environment.ts`
- Make sure you're accessing via `http://localhost:4200` (not different port)

### "Authentication failed" error
- Check that Google Client ID and Secret match in backend config
- Verify the redirect URI in Google Console matches `http://localhost:4200`
- Check browser console and backend logs for detailed errors

### 401 Unauthorized on CMS page
- Check if JWT token is in localStorage (Browser Dev Tools → Application → Local Storage)
- Verify JWT secret is configured in backend
- Try logging out and logging in again

### CORS errors
- Make sure backend is running on default port
- Check that frontend origin is in `appsettings.json` under `Cors:AllowedOrigins`

## What's Next?

Now that authentication is working:

1. **Customize the CMS**: Edit `/ClientApp/src/app/cms/cms.component.*`
2. **Add More Protected Routes**: Use the `AuthGuard` on other routes
3. **Implement CMS Features**: Add content management functionality in `/Api/Controllers/CmsController.cs`
4. **Deploy to Production**: See [AUTHENTICATION_SETUP.md](AUTHENTICATION_SETUP.md) for production configuration

## Quick Commands Reference

```bash
# Backend
dotnet build          # Build the backend
dotnet run            # Run the backend
dotnet test           # Run backend tests

# Frontend
cd ClientApp
npm install           # Install dependencies
npm start             # Run dev server
npm run build         # Build for production
npm test              # Run tests

# Both (from root)
dotnet build          # Builds both backend and frontend
```

## Architecture at a Glance

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│   Browser   │         │  Angular SPA │         │  ASP.NET    │
│             │         │   (Port 4200)│         │  (Port 5000)│
└──────┬──────┘         └──────┬───────┘         └──────┬──────┘
       │                       │                         │
       │ 1. Click "Login"      │                         │
       ├──────────────────────>│                         │
       │                       │                         │
       │ 2. Google Sign-In     │                         │
       │<──────────────────────┤                         │
       │                       │                         │
       │ 3. ID Token           │                         │
       ├──────────────────────>│ 4. POST /api/auth/google│
       │                       ├────────────────────────>│
       │                       │                         │
       │                       │ 5. JWT Token            │
       │                       │<────────────────────────┤
       │ 6. Store JWT          │                         │
       │<──────────────────────┤                         │
       │                       │                         │
       │ 7. Access /cms        │                         │
       ├──────────────────────>│ 8. GET /api/cms         │
       │                       │    + JWT Bearer Token   │
       │                       ├────────────────────────>│
       │                       │                         │
       │                       │ 9. CMS Data             │
       │ 10. Display CMS       │<────────────────────────┤
       │<──────────────────────┤                         │
       │                       │                         │
```

## Support

For detailed documentation, see:
- [AUTHENTICATION_README.md](AUTHENTICATION_README.md) - Architecture details
- [AUTHENTICATION_SETUP.md](AUTHENTICATION_SETUP.md) - Configuration guide
- [SECURITY_SUMMARY.md](SECURITY_SUMMARY.md) - Security information

Happy coding! 🚀
