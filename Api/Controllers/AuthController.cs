using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace bitsbybeier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback))
        };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning("Google authentication failed");
                return BadRequest(new { message = "Authentication failed" });
            }

            var email = authenticateResult.Principal?.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal?.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Email claim not found in authentication result");
                return BadRequest(new { message = "Email not found" });
            }

            // Generate JWT token
            var token = GenerateJwtToken(email, name);

            // Redirect to Angular app with token
            var redirectUrl = $"http://localhost:4200/auth-callback?token={token}";
            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google authentication callback");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromBody] TokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest(new { message = "Token is required" });
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSecretKey = _configuration["Authentication:Jwt:SecretKey"];

        if (string.IsNullOrEmpty(jwtSecretKey))
        {
            _logger.LogError("JWT SecretKey is not configured");
            return StatusCode(500, new { message = "Server configuration error" });
        }

        var key = Encoding.ASCII.GetBytes(jwtSecretKey);

        try
        {
            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Authentication:Jwt:Issuer"],
                ValidAudience = _configuration["Authentication:Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;
            var name = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;

            return Ok(new { email, name });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token validation failed");
            return Unauthorized(new { message = "Invalid token" });
        }
    }

    private string GenerateJwtToken(string email, string? name)
    {
        var jwtSecretKey = _configuration["Authentication:Jwt:SecretKey"];
        if (string.IsNullOrEmpty(jwtSecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSecretKey);
        var expirationMinutes = int.Parse(_configuration["Authentication:Jwt:ExpirationMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };

        if (!string.IsNullOrEmpty(name))
        {
            claims.Add(new Claim(ClaimTypes.Name, name));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Issuer = _configuration["Authentication:Jwt:Issuer"],
            Audience = _configuration["Authentication:Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class TokenRequest
{
    public string Token { get; set; } = string.Empty;
}
