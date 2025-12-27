using System.Security.Claims;

namespace bitsbybeier.Api.Services;

public interface IJwtTokenService
{
    string GenerateToken(IEnumerable<Claim> claims);
}
