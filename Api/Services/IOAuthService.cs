using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for managing OAuth 2.0 authentication flows.
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Validates client credentials.
    /// </summary>
    Task<OAuthClient?> ValidateClientAsync(string clientId, string clientSecret);
    
    /// <summary>
    /// Creates an authorization code.
    /// </summary>
    Task<string> CreateAuthorizationCodeAsync(
        string clientId, 
        int userId, 
        string redirectUri, 
        string scope,
        string? codeChallenge = null,
        string? codeChallengeMethod = null);
    
    /// <summary>
    /// Validates and consumes an authorization code.
    /// </summary>
    Task<OAuthAuthorizationCode?> ValidateAuthorizationCodeAsync(
        string code, 
        string clientId, 
        string redirectUri,
        string? codeVerifier = null);
    
    /// <summary>
    /// Creates access and refresh tokens.
    /// </summary>
    Task<(string accessToken, string refreshToken, int expiresIn)> CreateTokensAsync(
        string clientId, 
        int userId, 
        string scope);
    
    /// <summary>
    /// Validates and renews access token using refresh token.
    /// </summary>
    Task<(string accessToken, int expiresIn)?> RefreshAccessTokenAsync(
        string refreshToken, 
        string clientId);
    
    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    Task RevokeRefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// Validates redirect URI against client's allowed URIs.
    /// </summary>
    bool ValidateRedirectUri(OAuthClient client, string redirectUri);
    
    /// <summary>
    /// Validates requested scope against client's allowed scopes.
    /// </summary>
    bool ValidateScope(OAuthClient client, string scope);
}
