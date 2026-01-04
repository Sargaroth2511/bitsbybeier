# BitsbyBeier - Content Management System with MCP Integration

A modern, secure content management system built with ASP.NET Core and Angular, featuring OAuth 2.0 authentication and Model Context Protocol (MCP) integration for AI-powered content management.

## 🚀 Features

### Core Features
- ✅ Content creation and management
- ✅ Draft and publish workflow
- ✅ Markdown support
- ✅ Image management
- ✅ User authentication and authorization
- ✅ Role-based access control (Admin/User)

### MCP Server Integration
- ✅ Model Context Protocol (MCP) server for AI agents
- ✅ Content creation via MCP tools
- ✅ ChatGPT custom app integration
- ✅ OAuth 2.0 authentication for AI agents

### Security Features
- ✅ Google OAuth 2.0 authentication
- ✅ JWT-based API authentication
- ✅ OAuth 2.0 Authorization Code flow with PKCE
- ✅ Secure client credential management
- ✅ Role-based authorization
- ✅ Comprehensive audit logging

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [PostgreSQL 16](https://www.postgresql.org/download/)
- [Google Cloud account](https://console.cloud.google.com/) (for OAuth)

## 🏁 Quick Start

### 1. Clone and Setup

```bash
# Clone the repository
git clone https://github.com/Sargaroth2511/bitsbybeier.git
cd bitsbybeier

# Restore .NET packages
dotnet restore

# Install Node.js dependencies
cd ClientApp
npm install
cd ..
```

### 2. Configure Google OAuth

See [QUICKSTART.md](QUICKSTART.md) for detailed Google OAuth setup instructions.

### 3. Configure Database

```bash
# Start PostgreSQL
./start-database.sh

# Apply migrations
dotnet ef database update
```

See [DATABASE_SETUP.md](DATABASE_SETUP.md) for detailed database configuration.

### 4. Run the Application

```bash
# Start the application (backend + frontend)
dotnet run
```

The application will be available at:
- Frontend: http://localhost:4200
- Backend API: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

## 📚 Documentation

### Getting Started
- **[QUICKSTART.md](QUICKSTART.md)** - Get up and running in 10 minutes
- **[SETUP_COMPLETE.md](SETUP_COMPLETE.md)** - Complete setup guide
- **[DATABASE_SETUP.md](DATABASE_SETUP.md)** - Database configuration

### Authentication
- **[AUTHENTICATION_SETUP.md](AUTHENTICATION_SETUP.md)** - Google OAuth setup
- **[AUTHENTICATION_README.md](AUTHENTICATION_README.md)** - Authentication overview

### MCP Server
- **[MCP_CONTENT_API_README.md](MCP_CONTENT_API_README.md)** - MCP API documentation
- **[MCP_CLIENT_SETUP_GUIDE.md](MCP_CLIENT_SETUP_GUIDE.md)** - MCP client configuration

### OAuth 2.0 (NEW!)
- **[OAUTH_SETUP_GUIDE.md](OAUTH_SETUP_GUIDE.md)** - Complete OAuth 2.0 setup guide
- **[OAUTH_IMPLEMENTATION_STATUS.md](OAUTH_IMPLEMENTATION_STATUS.md)** - Implementation status
- **[OAUTH_SECURITY_SUMMARY.md](OAUTH_SECURITY_SUMMARY.md)** - Security analysis

### Development
- **[SWAGGER_TESTING_GUIDE.md](SWAGGER_TESTING_GUIDE.md)** - API testing with Swagger
- **[IMPLEMENTATION_SUMMARY.md](IMPLEMENTATION_SUMMARY.md)** - Technical implementation details

## 🤖 ChatGPT Integration

Connect ChatGPT to your BitsbyBeier instance using OAuth 2.0!

### Step 1: Create an OAuth Client

```bash
curl -X POST https://localhost:5001/api/oauth/admin/clients \
  -H "Authorization: Bearer YOUR_ADMIN_JWT" \
  -H "Content-Type: application/json" \
  -d '{
    "clientName": "ChatGPT MCP Integration",
    "redirectUris": ["https://chat.openai.com/aip/oauth/callback"],
    "allowedScopes": ["mcp:read", "mcp:write"]
  }'
```

### Step 2: Configure ChatGPT

In ChatGPT's custom app settings:
- **Authorization URL:** `https://your-domain.com/oauth/authorize`
- **Token URL:** `https://your-domain.com/oauth/token`
- **Client ID:** From Step 1
- **Client Secret:** From Step 1
- **Scope:** `mcp:read mcp:write`

### Step 3: Use ChatGPT to Manage Content

```
ChatGPT: "Create a blog post about TypeScript best practices"
→ Authenticates via OAuth 2.0
→ Uses MCP CreateContentAsync tool
→ Content created in your BitsbyBeier CMS!
```

See [OAUTH_SETUP_GUIDE.md](OAUTH_SETUP_GUIDE.md) for complete instructions.

## 🏗️ Architecture

```
┌─────────────┐      OAuth 2.0       ┌──────────────────┐
│  ChatGPT /  │◄───────────────────►│  BitsbyBeier     │
│  AI Agent   │      MCP Tools       │  Backend API     │
└─────────────┘                      └────────┬─────────┘
                                              │
┌─────────────┐                               │
│   Angular   │◄──────────────────────────────┤
│   Frontend  │      Google OAuth             │
└─────────────┘                               │
                                              ▼
                                     ┌─────────────────┐
                                     │   PostgreSQL    │
                                     │    Database     │
                                     └─────────────────┘
```

### Tech Stack

**Backend:**
- ASP.NET Core 9.0
- Entity Framework Core 9.0
- PostgreSQL 16
- JWT Authentication
- OAuth 2.0 with PKCE
- Model Context Protocol (MCP) 0.5.0

**Frontend:**
- Angular 18
- TypeScript
- Material Design
- Google Sign-In

## 🔐 Security

### Authentication Methods
1. **Google OAuth 2.0** - User authentication
2. **JWT Tokens** - API authentication
3. **OAuth 2.0 + PKCE** - AI agent authentication

### Security Features
- SHA256 password hashing for OAuth client secrets
- Short-lived authorization codes (10 minutes)
- Refresh token support (30 days)
- PKCE for public clients
- Comprehensive audit logging
- Role-based access control
- CORS protection
- HTTPS enforcement

### Security Compliance
- ✅ OAuth 2.0 (RFC 6749)
- ✅ PKCE (RFC 7636)
- ✅ OAuth 2.0 Security Best Practices (RFC 9700)
- ✅ OWASP OAuth Security Guidelines
- ✅ CodeQL security scan passed (0 vulnerabilities)

See [OAUTH_SECURITY_SUMMARY.md](OAUTH_SECURITY_SUMMARY.md) for detailed security analysis.

## 📊 API Endpoints

### Authentication
- `POST /api/auth/google` - Google OAuth login
- `GET /api/auth/user` - Get current user

### Content Management
- `GET /api/cms/content` - List all content (Admin)
- `POST /api/cms/content` - Create content (Admin)
- `GET /api/cms/content/public` - List public content
- `GET /api/cms/content/drafts` - List drafts (Admin)

### MCP Server
- `GET /api/mcp/info` - Get MCP server info
- `POST /api/mcp/tools` - Execute MCP tool

### OAuth 2.0
- `GET /oauth/authorize` - Authorization endpoint
- `POST /oauth/token` - Token endpoint
- `POST /oauth/revoke` - Token revocation

### OAuth Management (Admin)
- `POST /api/oauth/admin/clients` - Create OAuth client
- `GET /api/oauth/admin/clients` - List OAuth clients
- `DELETE /api/oauth/admin/clients/{id}` - Deactivate client

## 🧪 Testing

### Using Swagger UI

1. Navigate to https://localhost:5001/swagger
2. Click "Authorize" button
3. Enter your JWT token
4. Test endpoints interactively

See [SWAGGER_TESTING_GUIDE.md](SWAGGER_TESTING_GUIDE.md) for details.

### Using curl

```bash
# Get JWT token
curl -X POST https://localhost:5001/api/auth/google \
  -H "Content-Type: application/json" \
  -d '{"idToken": "YOUR_GOOGLE_ID_TOKEN"}'

# Create content
curl -X POST https://localhost:5001/api/cms/content \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "author": "John Doe",
    "title": "My First Post",
    "content": "Hello, world!",
    "draft": true
  }'
```

## 📦 Database Schema

### Core Tables
- `Users` - User accounts and profiles
- `UserImages` - User profile images
- `Contents` - Content items (posts, articles)
- `ContentImages` - Content images

### OAuth Tables (NEW!)
- `OAuthClients` - OAuth 2.0 client applications
- `OAuthAuthorizationCodes` - Authorization codes
- `OAuthRefreshTokens` - Refresh tokens

## 🚢 Deployment

### Development
```bash
dotnet run
```

### Production

1. Update `appsettings.json` with production values
2. Set environment variables for secrets
3. Configure SSL/TLS certificates
4. Set up reverse proxy (nginx/Apache)
5. Configure production CORS policies
6. Enable rate limiting
7. Set up monitoring and logging

See [SERVER_DATABASE_SETUP.md](SERVER_DATABASE_SETUP.md) for server deployment.

## 🔧 Configuration

### Environment Variables

```bash
# Database
DB_HOST=localhost
DB_PORT=5432
DB_USER=bitsbybeier
DB_PASSWORD=your_password

# Authentication
GOOGLE_CLIENT_ID=your_client_id
GOOGLE_CLIENT_SECRET=your_client_secret
JWT_SECRET=your_jwt_secret

# CORS
CORS_ALLOWED_ORIGINS=https://your-domain.com

# Initial Admin
INITIAL_ADMIN_EMAIL=admin@example.com
INITIAL_ADMIN_DISPLAYNAME=Admin User
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests and security scans
5. Submit a pull request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

- **Issues:** [GitHub Issues](https://github.com/Sargaroth2511/bitsbybeier/issues)
- **Documentation:** See [docs](.) directory
- **Security:** See [OAUTH_SECURITY_SUMMARY.md](OAUTH_SECURITY_SUMMARY.md)

## 🎯 Roadmap

### Completed
- ✅ Core CMS functionality
- ✅ Google OAuth authentication
- ✅ MCP server integration
- ✅ OAuth 2.0 for AI agents
- ✅ PKCE support
- ✅ Security hardening

### Planned
- 🔲 Consent page UI for OAuth
- 🔲 Rate limiting
- 🔲 Refresh token rotation
- 🔲 Content versioning
- 🔲 Rich text editor
- 🔲 Image upload UI
- 🔲 Content categories
- 🔲 Search functionality

## 📅 Latest Updates

### January 4, 2026
- ✅ Implemented OAuth 2.0 Authorization Code flow
- ✅ Added PKCE support
- ✅ Created OAuth management endpoints
- ✅ Added comprehensive documentation
- ✅ Security scan passed (0 vulnerabilities)

---

**Built with ❤️ using ASP.NET Core and Angular**
