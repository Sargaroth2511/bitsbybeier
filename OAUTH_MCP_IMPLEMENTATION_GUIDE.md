# OAuth Flow Implementation Guide for ChatGPT MCP Integration

## Overview

This guide provides detailed instructions for implementing OAuth 2.0 authentication flow to enable ChatGPT custom apps to connect to the BitsbyBeier MCP server. The current implementation uses JWT bearer tokens, but ChatGPT's custom app interface expects a standard OAuth 2.0 flow with client credentials.

## Current State

### What We Have
- JWT-based authentication via Google OAuth
- Bearer token authentication on MCP endpoints
- Admin role-based authorization
- MCP server at `/api/mcp/*` endpoints

### What We Need
- OAuth 2.0 Authorization Code flow
- Client credentials (Client ID and Client Secret) management
- OAuth authorization endpoint
- OAuth token endpoint
- Token refresh mechanism

## OAuth 2.0 Flow Requirements

### ChatGPT Custom App Expectations

ChatGPT's custom app configuration expects:
1. **Authorization URL**: Where users grant access (e.g., `/oauth/authorize`)
2. **Token URL**: Where the app exchanges auth code for tokens (e.g., `/oauth/token`)
3. **Client ID**: Unique identifier for the ChatGPT app
4. **Client Secret**: Secret key for secure communication
5. **Scopes**: Permissions being requested (e.g., `mcp:read`, `mcp:write`)

### Flow Diagram

```
User                ChatGPT App           BitsbyBeier Server
  |                      |                         |
  |--1. Click "Connect"->|                         |
  |                      |--2. Redirect to Auth--->|
  |<-----------------3. Show Login Page------------|
  |--4. Login & Approve-------------------------------->|
  |                      |<--5. Auth Code-----------|
  |                      |--6. Exchange Code------->|
  |                      |<--7. Access Token--------|
  |                      |--8. Use MCP Tools------->|
  |                      |    (with token)          |
```

## Implementation Steps

### Step 1: Database Schema Changes

#### 1.1 Create OAuth Client Table

Create a new migration to add OAuth client management:

```csharp
// File: Migrations/YYYYMMDDHHMMSS_AddOAuthClients.cs

public partial class AddOAuthClients : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OAuthClients",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ClientId = table.Column<string>(maxLength: 100, nullable: false),
                ClientSecret = table.Column<string>(maxLength: 500, nullable: false),
                ClientName = table.Column<string>(maxLength: 200, nullable: false),
                RedirectUris = table.Column<string>(nullable: false), // JSON array
                AllowedScopes = table.Column<string>(nullable: false), // JSON array
                Active = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValue: DateTime.UtcNow),
                CreatedByUserId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OAuthClients", x => x.Id);
                table.UniqueConstraint("AK_OAuthClients_ClientId", x => x.ClientId);
                table.ForeignKey(
                    name: "FK_OAuthClients_Users_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OAuthClients_ClientId",
            table: "OAuthClients",
            column: "ClientId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "OAuthClients");
    }
}
```

#### 1.2 Create Authorization Codes Table

```csharp
// File: Migrations/YYYYMMDDHHMMSS_AddOAuthAuthorizationCodes.cs

public partial class AddOAuthAuthorizationCodes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OAuthAuthorizationCodes",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Code = table.Column<string>(maxLength: 100, nullable: false),
                ClientId = table.Column<string>(maxLength: 100, nullable: false),
                UserId = table.Column<int>(nullable: false),
                RedirectUri = table.Column<string>(maxLength: 500, nullable: false),
                Scope = table.Column<string>(maxLength: 500, nullable: false),
                CodeChallenge = table.Column<string>(maxLength: 100, nullable: true), // PKCE
                CodeChallengeMethod = table.Column<string>(maxLength: 10, nullable: true), // PKCE
                ExpiresAt = table.Column<DateTime>(nullable: false),
                Used = table.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValue: DateTime.UtcNow)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OAuthAuthorizationCodes", x => x.Id);
                table.UniqueConstraint("AK_OAuthAuthorizationCodes_Code", x => x.Code);
                table.ForeignKey(
                    name: "FK_OAuthAuthorizationCodes_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OAuthAuthorizationCodes_Code",
            table: "OAuthAuthorizationCodes",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OAuthAuthorizationCodes_ExpiresAt",
            table: "OAuthAuthorizationCodes",
            column: "ExpiresAt");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "OAuthAuthorizationCodes");
    }
}
```

#### 1.3 Create Refresh Tokens Table

```csharp
// File: Migrations/YYYYMMDDHHMMSS_AddOAuthRefreshTokens.cs

public partial class AddOAuthRefreshTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OAuthRefreshTokens",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Token = table.Column<string>(maxLength: 500, nullable: false),
                ClientId = table.Column<string>(maxLength: 100, nullable: false),
                UserId = table.Column<int>(nullable: false),
                Scope = table.Column<string>(maxLength: 500, nullable: false),
                ExpiresAt = table.Column<DateTime>(nullable: false),
                Revoked = table.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValue: DateTime.UtcNow)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OAuthRefreshTokens", x => x.Id);
                table.UniqueConstraint("AK_OAuthRefreshTokens_Token", x => x.Token);
                table.ForeignKey(
                    name: "FK_OAuthRefreshTokens_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OAuthRefreshTokens_Token",
            table: "OAuthRefreshTokens",
            column: "Token",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "OAuthRefreshTokens");
    }
}
```

### Step 2: Domain Models

#### 2.1 Create OAuthClient Model

```csharp
// File: Domain/Models/OAuthClient.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents an OAuth 2.0 client application that can access the MCP server.
/// </summary>
public class OAuthClient
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string ClientSecret { get; set; } = string.Empty; // Should be hashed
    
    [Required]
    [MaxLength(200)]
    public string ClientName { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON array of allowed redirect URIs
    /// </summary>
    [Required]
    public string RedirectUris { get; set; } = "[]";
    
    /// <summary>
    /// JSON array of allowed scopes (e.g., ["mcp:read", "mcp:write"])
    /// </summary>
    [Required]
    public string AllowedScopes { get; set; } = "[]";
    
    public bool Active { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public int CreatedByUserId { get; set; }
    
    public User? CreatedBy { get; set; }
}
```

#### 2.2 Create OAuthAuthorizationCode Model

```csharp
// File: Domain/Models/OAuthAuthorizationCode.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents a short-lived authorization code for OAuth 2.0 flow.
/// </summary>
public class OAuthAuthorizationCode
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string RedirectUri { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Scope { get; set; } = string.Empty;
    
    /// <summary>
    /// PKCE code challenge (optional)
    /// </summary>
    [MaxLength(100)]
    public string? CodeChallenge { get; set; }
    
    /// <summary>
    /// PKCE challenge method (S256 or plain)
    /// </summary>
    [MaxLength(10)]
    public string? CodeChallengeMethod { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public bool Used { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

#### 2.3 Create OAuthRefreshToken Model

```csharp
// File: Domain/Models/OAuthRefreshToken.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bitsbybeier.Domain.Models;

/// <summary>
/// Represents a refresh token for OAuth 2.0 token renewal.
/// </summary>
public class OAuthRefreshToken
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ClientId { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Scope { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool Revoked { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Step 3: Update ApplicationDbContext

```csharp
// File: Data/ApplicationDbContext.cs

// Add these DbSets to the ApplicationDbContext class:

public DbSet<OAuthClient> OAuthClients => Set<OAuthClient>();
public DbSet<OAuthAuthorizationCode> OAuthAuthorizationCodes => Set<OAuthAuthorizationCode>();
public DbSet<OAuthRefreshToken> OAuthRefreshTokens => Set<OAuthRefreshToken>();

// Add indexes in OnModelCreating:
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // ... existing configurations ...
    
    // OAuth Client indexes
    modelBuilder.Entity<OAuthClient>()
        .HasIndex(c => c.ClientId)
        .IsUnique();
    
    // OAuth Authorization Code indexes
    modelBuilder.Entity<OAuthAuthorizationCode>()
        .HasIndex(c => c.Code)
        .IsUnique();
    
    modelBuilder.Entity<OAuthAuthorizationCode>()
        .HasIndex(c => c.ExpiresAt);
    
    // OAuth Refresh Token indexes
    modelBuilder.Entity<OAuthRefreshToken>()
        .HasIndex(t => t.Token)
        .IsUnique();
}
```

### Step 4: Create OAuth Service

#### 4.1 Create IOAuthService Interface

```csharp
// File: Api/Services/IOAuthService.cs

using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

public interface IOAuthService
{
    /// <summary>
    /// Validates client credentials
    /// </summary>
    Task<OAuthClient?> ValidateClientAsync(string clientId, string clientSecret);
    
    /// <summary>
    /// Creates an authorization code
    /// </summary>
    Task<string> CreateAuthorizationCodeAsync(
        string clientId, 
        int userId, 
        string redirectUri, 
        string scope,
        string? codeChallenge = null,
        string? codeChallengeMethod = null);
    
    /// <summary>
    /// Validates and consumes an authorization code
    /// </summary>
    Task<OAuthAuthorizationCode?> ValidateAuthorizationCodeAsync(
        string code, 
        string clientId, 
        string redirectUri,
        string? codeVerifier = null);
    
    /// <summary>
    /// Creates access and refresh tokens
    /// </summary>
    Task<(string accessToken, string refreshToken, int expiresIn)> CreateTokensAsync(
        string clientId, 
        int userId, 
        string scope);
    
    /// <summary>
    /// Validates and renews access token using refresh token
    /// </summary>
    Task<(string accessToken, int expiresIn)?> RefreshAccessTokenAsync(
        string refreshToken, 
        string clientId);
    
    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// Validates redirect URI against client's allowed URIs
    /// </summary>
    bool ValidateRedirectUri(OAuthClient client, string redirectUri);
    
    /// <summary>
    /// Validates requested scope against client's allowed scopes
    /// </summary>
    bool ValidateScope(OAuthClient client, string scope);
}
```

#### 4.2 Create OAuthService Implementation

```csharp
// File: Api/Services/OAuthService.cs

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using bitsbybeier.Data;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

public class OAuthService : IOAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<OAuthService> _logger;
    
    // Authorization codes expire in 10 minutes
    private static readonly TimeSpan AuthCodeExpiration = TimeSpan.FromMinutes(10);
    
    // Refresh tokens expire in 30 days
    private static readonly TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(30);
    
    public OAuthService(
        ApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        ILogger<OAuthService> logger)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }
    
    public async Task<OAuthClient?> ValidateClientAsync(string clientId, string clientSecret)
    {
        var client = await _context.OAuthClients
            .FirstOrDefaultAsync(c => c.ClientId == clientId && c.Active);
            
        if (client == null)
        {
            _logger.LogWarning("Client validation failed: client not found or inactive. ClientId: {ClientId}", clientId);
            return null;
        }
        
        // Hash the provided secret and compare
        var hashedSecret = HashClientSecret(clientSecret);
        if (client.ClientSecret != hashedSecret)
        {
            _logger.LogWarning("Client validation failed: invalid secret. ClientId: {ClientId}", clientId);
            return null;
        }
        
        return client;
    }
    
    public async Task<string> CreateAuthorizationCodeAsync(
        string clientId,
        int userId,
        string redirectUri,
        string scope,
        string? codeChallenge = null,
        string? codeChallengeMethod = null)
    {
        var code = GenerateSecureToken();
        
        var authCode = new OAuthAuthorizationCode
        {
            Code = code,
            ClientId = clientId,
            UserId = userId,
            RedirectUri = redirectUri,
            Scope = scope,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            ExpiresAt = DateTime.UtcNow.Add(AuthCodeExpiration),
            Used = false
        };
        
        _context.OAuthAuthorizationCodes.Add(authCode);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Authorization code created for ClientId: {ClientId}, UserId: {UserId}", clientId, userId);
        
        return code;
    }
    
    public async Task<OAuthAuthorizationCode?> ValidateAuthorizationCodeAsync(
        string code,
        string clientId,
        string redirectUri,
        string? codeVerifier = null)
    {
        var authCode = await _context.OAuthAuthorizationCodes
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => 
                c.Code == code && 
                c.ClientId == clientId && 
                c.RedirectUri == redirectUri &&
                !c.Used);
        
        if (authCode == null)
        {
            _logger.LogWarning("Authorization code validation failed: not found or already used");
            return null;
        }
        
        if (authCode.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Authorization code validation failed: expired");
            return null;
        }
        
        // Validate PKCE if code challenge was provided
        if (!string.IsNullOrEmpty(authCode.CodeChallenge))
        {
            if (string.IsNullOrEmpty(codeVerifier))
            {
                _logger.LogWarning("Authorization code validation failed: PKCE verifier missing");
                return null;
            }
            
            if (!ValidatePKCE(authCode.CodeChallenge, codeVerifier, authCode.CodeChallengeMethod))
            {
                _logger.LogWarning("Authorization code validation failed: invalid PKCE verifier");
                return null;
            }
        }
        
        // Mark as used
        authCode.Used = true;
        await _context.SaveChangesAsync();
        
        return authCode;
    }
    
    public async Task<(string accessToken, string refreshToken, int expiresIn)> CreateTokensAsync(
        string clientId,
        int userId,
        string scope)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }
        
        // Create access token (JWT)
        var accessToken = _jwtTokenService.GenerateToken(user);
        var expiresIn = 3600; // 1 hour
        
        // Create refresh token
        var refreshToken = GenerateSecureToken();
        
        var refreshTokenEntity = new OAuthRefreshToken
        {
            Token = refreshToken,
            ClientId = clientId,
            UserId = userId,
            Scope = scope,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenExpiration),
            Revoked = false
        };
        
        _context.OAuthRefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Tokens created for ClientId: {ClientId}, UserId: {UserId}", clientId, userId);
        
        return (accessToken, refreshToken, expiresIn);
    }
    
    public async Task<(string accessToken, int expiresIn)?> RefreshAccessTokenAsync(
        string refreshToken,
        string clientId)
    {
        var tokenEntity = await _context.OAuthRefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => 
                t.Token == refreshToken && 
                t.ClientId == clientId && 
                !t.Revoked);
        
        if (tokenEntity == null)
        {
            _logger.LogWarning("Refresh token validation failed: not found or revoked");
            return null;
        }
        
        if (tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token validation failed: expired");
            return null;
        }
        
        if (tokenEntity.User == null)
        {
            _logger.LogError("Refresh token validation failed: user not found");
            return null;
        }
        
        // Generate new access token
        var accessToken = _jwtTokenService.GenerateToken(tokenEntity.User);
        var expiresIn = 3600; // 1 hour
        
        _logger.LogInformation("Access token refreshed for ClientId: {ClientId}, UserId: {UserId}", 
            clientId, tokenEntity.UserId);
        
        return (accessToken, expiresIn);
    }
    
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.OAuthRefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken);
        
        if (tokenEntity != null)
        {
            tokenEntity.Revoked = true;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Refresh token revoked: {Token}", refreshToken);
        }
    }
    
    public bool ValidateRedirectUri(OAuthClient client, string redirectUri)
    {
        var allowedUris = JsonSerializer.Deserialize<List<string>>(client.RedirectUris);
        return allowedUris?.Contains(redirectUri) ?? false;
    }
    
    public bool ValidateScope(OAuthClient client, string scope)
    {
        var allowedScopes = JsonSerializer.Deserialize<List<string>>(client.AllowedScopes);
        var requestedScopes = scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        return requestedScopes.All(s => allowedScopes?.Contains(s) ?? false);
    }
    
    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
    
    private static string HashClientSecret(string secret)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(secret);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    private static bool ValidatePKCE(string challenge, string verifier, string? method)
    {
        if (method == "S256")
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(verifier);
            var hash = sha256.ComputeHash(bytes);
            var computedChallenge = Convert.ToBase64String(hash)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
            return computedChallenge == challenge;
        }
        else if (method == "plain" || string.IsNullOrEmpty(method))
        {
            return verifier == challenge;
        }
        
        return false;
    }
}
```

### Step 5: Create OAuth Controller

```csharp
// File: Api/Controllers/OAuthController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using bitsbybeier.Api.Services;
using bitsbybeier.Data;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// OAuth 2.0 endpoints for authorization and token management.
/// </summary>
[Route("oauth")]
[ApiController]
public class OAuthController : BaseController
{
    private readonly IOAuthService _oauthService;
    
    public OAuthController(
        ILogger<OAuthController> logger,
        ApplicationDbContext context,
        IOAuthService oauthService)
        : base(logger, context)
    {
        _oauthService = oauthService;
    }
    
    /// <summary>
    /// OAuth 2.0 authorization endpoint.
    /// Displays consent page and generates authorization code.
    /// </summary>
    [HttpGet("authorize")]
    [Authorize] // User must be logged in
    public async Task<IActionResult> Authorize(
        [FromQuery] string response_type,
        [FromQuery] string client_id,
        [FromQuery] string redirect_uri,
        [FromQuery] string? scope,
        [FromQuery] string? state,
        [FromQuery] string? code_challenge,
        [FromQuery] string? code_challenge_method)
    {
        // Validate response_type
        if (response_type != "code")
        {
            return BadRequest(new { error = "unsupported_response_type" });
        }
        
        // Validate client
        var client = await Context.OAuthClients
            .FirstOrDefaultAsync(c => c.ClientId == client_id && c.Active);
            
        if (client == null)
        {
            return BadRequest(new { error = "invalid_client" });
        }
        
        // Validate redirect URI
        if (!_oauthService.ValidateRedirectUri(client, redirect_uri))
        {
            return BadRequest(new { error = "invalid_redirect_uri" });
        }
        
        // Validate scope
        scope = scope ?? "mcp:read mcp:write";
        if (!_oauthService.ValidateScope(client, scope))
        {
            return BadRequest(new { error = "invalid_scope" });
        }
        
        // In a real implementation, show a consent page here
        // For now, we'll auto-approve if user has Admin role
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }
        
        // Create authorization code
        var code = await _oauthService.CreateAuthorizationCodeAsync(
            client_id,
            UserId,
            redirect_uri,
            scope,
            code_challenge,
            code_challenge_method);
        
        // Redirect back to client with code
        var redirectUrl = $"{redirect_uri}?code={code}";
        if (!string.IsNullOrEmpty(state))
        {
            redirectUrl += $"&state={state}";
        }
        
        return Redirect(redirectUrl);
    }
    
    /// <summary>
    /// OAuth 2.0 token endpoint.
    /// Exchanges authorization code for access token.
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<IActionResult> Token(
        [FromForm] string grant_type,
        [FromForm] string? code,
        [FromForm] string? redirect_uri,
        [FromForm] string? client_id,
        [FromForm] string? client_secret,
        [FromForm] string? refresh_token,
        [FromForm] string? code_verifier)
    {
        if (string.IsNullOrEmpty(client_id) || string.IsNullOrEmpty(client_secret))
        {
            // Try Basic Auth
            var authHeader = Request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Basic "))
            {
                var encoded = authHeader.Substring("Basic ".Length);
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var parts = decoded.Split(':', 2);
                client_id = parts[0];
                client_secret = parts.Length > 1 ? parts[1] : "";
            }
        }
        
        if (string.IsNullOrEmpty(client_id) || string.IsNullOrEmpty(client_secret))
        {
            return BadRequest(new { error = "invalid_request", error_description = "Missing client credentials" });
        }
        
        // Validate client
        var client = await _oauthService.ValidateClientAsync(client_id, client_secret);
        if (client == null)
        {
            return Unauthorized(new { error = "invalid_client" });
        }
        
        if (grant_type == "authorization_code")
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(redirect_uri))
            {
                return BadRequest(new { error = "invalid_request" });
            }
            
            // Validate and consume authorization code
            var authCode = await _oauthService.ValidateAuthorizationCodeAsync(
                code,
                client_id,
                redirect_uri,
                code_verifier);
            
            if (authCode == null)
            {
                return BadRequest(new { error = "invalid_grant" });
            }
            
            // Create tokens
            var (accessToken, refreshToken, expiresIn) = await _oauthService.CreateTokensAsync(
                client_id,
                authCode.UserId,
                authCode.Scope);
            
            return Ok(new
            {
                access_token = accessToken,
                token_type = "Bearer",
                expires_in = expiresIn,
                refresh_token = refreshToken,
                scope = authCode.Scope
            });
        }
        else if (grant_type == "refresh_token")
        {
            if (string.IsNullOrEmpty(refresh_token))
            {
                return BadRequest(new { error = "invalid_request" });
            }
            
            // Refresh access token
            var result = await _oauthService.RefreshAccessTokenAsync(refresh_token, client_id);
            
            if (result == null)
            {
                return BadRequest(new { error = "invalid_grant" });
            }
            
            return Ok(new
            {
                access_token = result.Value.accessToken,
                token_type = "Bearer",
                expires_in = result.Value.expiresIn
            });
        }
        
        return BadRequest(new { error = "unsupported_grant_type" });
    }
    
    /// <summary>
    /// OAuth 2.0 token revocation endpoint.
    /// </summary>
    [HttpPost("revoke")]
    [AllowAnonymous]
    public async Task<IActionResult> Revoke(
        [FromForm] string token,
        [FromForm] string? client_id,
        [FromForm] string? client_secret)
    {
        // Validate client (if provided)
        if (!string.IsNullOrEmpty(client_id) && !string.IsNullOrEmpty(client_secret))
        {
            var client = await _oauthService.ValidateClientAsync(client_id, client_secret);
            if (client == null)
            {
                return Unauthorized();
            }
        }
        
        await _oauthService.RevokeRefreshTokenAsync(token);
        
        return Ok();
    }
}
```

### Step 6: Register Services in Program.cs

```csharp
// File: Program.cs

// Add this in the services registration section:

builder.Services.AddScoped<IOAuthService, OAuthService>();
```

### Step 7: Create Admin Endpoint for OAuth Client Management

```csharp
// File: Api/Controllers/OAuthManagementController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using bitsbybeier.Api.Services;
using bitsbybeier.Domain.Models;
using bitsbybeier.Data;
using System.Text.Json;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Admin endpoints for managing OAuth clients.
/// </summary>
[Route("api/oauth/admin")]
[Authorize(Roles = "Admin")]
[ApiController]
public class OAuthManagementController : BaseController
{
    public OAuthManagementController(
        ILogger<OAuthManagementController> logger,
        ApplicationDbContext context)
        : base(logger, context)
    {
    }
    
    /// <summary>
    /// Creates a new OAuth client for connecting external applications.
    /// </summary>
    [HttpPost("clients")]
    public async Task<IActionResult> CreateClient([FromBody] CreateOAuthClientRequest request)
    {
        var clientId = Guid.NewGuid().ToString("N");
        var clientSecret = GenerateClientSecret();
        
        var client = new OAuthClient
        {
            ClientId = clientId,
            ClientSecret = HashClientSecret(clientSecret), // Store hashed
            ClientName = request.ClientName,
            RedirectUris = JsonSerializer.Serialize(request.RedirectUris),
            AllowedScopes = JsonSerializer.Serialize(request.AllowedScopes ?? new[] { "mcp:read", "mcp:write" }),
            Active = true,
            CreatedByUserId = UserId
        };
        
        Context.OAuthClients.Add(client);
        await Context.SaveChangesAsync();
        
        Logger.LogInformation("OAuth client created: {ClientId} by user {UserId}", clientId, UserId);
        
        // Return client secret ONLY once, it won't be retrievable later
        return Ok(new
        {
            clientId = clientId,
            clientSecret = clientSecret, // Plain text, shown only once
            clientName = client.ClientName,
            redirectUris = request.RedirectUris,
            allowedScopes = request.AllowedScopes,
            message = "Save the client secret securely. It cannot be retrieved later."
        });
    }
    
    /// <summary>
    /// Lists all OAuth clients (without secrets).
    /// </summary>
    [HttpGet("clients")]
    public async Task<IActionResult> ListClients()
    {
        var clients = await Context.OAuthClients
            .Where(c => c.Active)
            .Select(c => new
            {
                c.ClientId,
                c.ClientName,
                RedirectUris = JsonSerializer.Deserialize<List<string>>(c.RedirectUris),
                AllowedScopes = JsonSerializer.Deserialize<List<string>>(c.AllowedScopes),
                c.CreatedAt
            })
            .ToListAsync();
        
        return Ok(clients);
    }
    
    /// <summary>
    /// Deactivates an OAuth client.
    /// </summary>
    [HttpDelete("clients/{clientId}")]
    public async Task<IActionResult> DeactivateClient(string clientId)
    {
        var client = await Context.OAuthClients
            .FirstOrDefaultAsync(c => c.ClientId == clientId);
        
        if (client == null)
        {
            return NotFound();
        }
        
        client.Active = false;
        await Context.SaveChangesAsync();
        
        Logger.LogInformation("OAuth client deactivated: {ClientId} by user {UserId}", clientId, UserId);
        
        return Ok();
    }
    
    private static string GenerateClientSecret()
    {
        var randomBytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
    
    private static string HashClientSecret(string secret)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(secret);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

public record CreateOAuthClientRequest(
    string ClientName,
    List<string> RedirectUris,
    List<string>? AllowedScopes);
```

### Step 8: Configuration Changes

#### 8.1 Update appsettings.json

```json
// Add OAuth configuration section:
{
  "OAuth": {
    "AuthorizationCodeExpirationMinutes": 10,
    "RefreshTokenExpirationDays": 30,
    "DefaultScopes": ["mcp:read", "mcp:write"]
  }
}
```

### Step 9: Testing the Implementation

#### 9.1 Create a Test OAuth Client

```bash
curl -k -X POST https://localhost:5001/api/oauth/admin/clients \
  -H "Authorization: Bearer YOUR_ADMIN_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "clientName": "ChatGPT Custom App",
    "redirectUris": [
      "https://chat.openai.com/aip/oauth/callback",
      "http://localhost:3000/callback"
    ],
    "allowedScopes": ["mcp:read", "mcp:write"]
  }'
```

Response:
```json
{
  "clientId": "abc123...",
  "clientSecret": "xyz789...",
  "clientName": "ChatGPT Custom App",
  "redirectUris": ["https://chat.openai.com/aip/oauth/callback"],
  "allowedScopes": ["mcp:read", "mcp:write"],
  "message": "Save the client secret securely. It cannot be retrieved later."
}
```

#### 9.2 Test Authorization Flow

```bash
# Step 1: Get authorization code
curl -k "https://localhost:5001/oauth/authorize?\
response_type=code&\
client_id=abc123...&\
redirect_uri=http://localhost:3000/callback&\
scope=mcp:read%20mcp:write&\
state=random_state" \
  -H "Authorization: Bearer YOUR_USER_JWT_TOKEN"

# Step 2: Exchange code for token
curl -k -X POST https://localhost:5001/oauth/token \
  -d "grant_type=authorization_code" \
  -d "code=AUTH_CODE_FROM_STEP1" \
  -d "redirect_uri=http://localhost:3000/callback" \
  -d "client_id=abc123..." \
  -d "client_secret=xyz789..."
```

### Step 10: ChatGPT Custom App Configuration

After implementing the above, configure ChatGPT:

**Name:** BitsbyBeier Content Manager

**Beschreibung:** Creates and manages blog content in the BitsbyBeier CMS

**URL des MCP-Servers:** `https://your-domain.com/api/mcp`

**Authentifizierung:** OAuth

**Authorization URL:** `https://your-domain.com/oauth/authorize`

**Token URL:** `https://your-domain.com/oauth/token`

**OAuth-Client-ID:** `abc123...` (from step 9.1)

**OAuth-Client-Geheimnis:** `xyz789...` (from step 9.1)

**Scope:** `mcp:read mcp:write`

## Security Considerations

1. **Client Secrets**: Always hash before storing in database
2. **Authorization Codes**: Short expiration time (10 minutes)
3. **PKCE Support**: Implemented for public clients
4. **Refresh Token Rotation**: Consider implementing token rotation
5. **Rate Limiting**: Add rate limiting to OAuth endpoints
6. **CORS**: Configure properly for production
7. **HTTPS**: Always use HTTPS in production
8. **Audit Logging**: Log all OAuth operations

## Production Checklist

- [ ] Implement consent page UI for authorization endpoint
- [ ] Add rate limiting to prevent brute force attacks
- [ ] Implement refresh token rotation
- [ ] Add comprehensive audit logging
- [ ] Set up monitoring and alerts
- [ ] Configure proper CORS policies
- [ ] Use production-grade secret storage (Azure Key Vault, AWS Secrets Manager)
- [ ] Implement token cleanup job (remove expired tokens)
- [ ] Add support for scope-based access control
- [ ] Document API for OAuth clients

## Migration Commands

```bash
# Create migrations
dotnet ef migrations add AddOAuthClients
dotnet ef migrations add AddOAuthAuthorizationCodes
dotnet ef migrations add AddOAuthRefreshTokens

# Apply migrations
dotnet ef database update
```

## Additional Resources

- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [OAuth 2.1 Draft](https://oauth.net/2.1/)
- [OpenID Connect](https://openid.net/connect/)

---

**Note**: This implementation provides a complete OAuth 2.0 Authorization Code flow with PKCE support. Review and adjust according to your specific security requirements and compliance needs.
