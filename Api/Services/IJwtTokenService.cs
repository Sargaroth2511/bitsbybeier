using System.Security.Claims;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for generating and managing JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token with the specified claims.
    /// </summary>
    /// <param name="claims">Collection of claims to include in the token.</param>
    /// <returns>A signed JWT token string.</returns>
    string GenerateToken(IEnumerable<Claim> claims);
}
