using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bitsbybeier.Api.Services;
using bitsbybeier.Data;
using System.Text;

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
        
        // Get user ID from claims
        var userIdClaim = UserId;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "invalid_user" });
        }
        
        // TODO: In production, show a consent page here instead of auto-approving
        // For now, we'll auto-approve if user has Admin role for development purposes
        // This allows ChatGPT and other OAuth clients to complete the flow
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }
        
        // Create authorization code
        var code = await _oauthService.CreateAuthorizationCodeAsync(
            client_id,
            userId,
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
