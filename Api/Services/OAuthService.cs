using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using bitsbybeier.Data;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Implementation of OAuth 2.0 authentication service with PKCE support.
/// </summary>
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
    
    public static string HashClientSecret(string secret)
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
