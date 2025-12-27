using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using bitsbybeier.Api.Configuration;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for generating JWT tokens using configured security settings.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<JwtTokenService> _logger;

    /// <summary>
    /// Initializes a new instance of the JwtTokenService.
    /// </summary>
    /// <param name="jwtOptions">JWT configuration options.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public JwtTokenService(IOptions<JwtOptions> jwtOptions, ILogger<JwtTokenService> logger)
    {
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Generates a JWT token with the specified user claims.
    /// </summary>
    /// <param name="userClaims">Claims to include in the token.</param>
    /// <returns>A signed JWT token string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when JWT secret is not configured.</exception>
    public string GenerateToken(IEnumerable<Claim> userClaims)
    {
        if (string.IsNullOrEmpty(_jwtOptions.Secret))
        {
            throw new InvalidOperationException("JWT Secret not configured");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>(userClaims)
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes),
            signingCredentials: credentials
        );

        _logger.LogDebug("Generated JWT token for {ClaimCount} claims with {ExpirationMinutes} minute expiration", 
            claims.Count, _jwtOptions.ExpirationMinutes);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
