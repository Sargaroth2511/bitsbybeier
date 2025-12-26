namespace bitsbybeier.Api.Services;

public interface IJwtTokenService
{
    string GenerateToken(string email, string name);
}
