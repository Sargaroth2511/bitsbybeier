using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using bitsbybeier.Api.Services;
using System.Security.Claims;
using Google.Apis.Auth;

namespace bitsbybeier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AuthController(IJwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleTokenRequest request)
    {
        try
        {
            // Verify the Google ID token
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured") }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
            
            if (payload == null)
            {
                return Unauthorized(new { message = "Invalid Google token" });
            }

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(payload.Email, payload.Name);

            return Ok(new { token, email = payload.Email, name = payload.Name });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = "Authentication failed", error = ex.Message });
        }
    }

    [HttpGet("user")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult GetUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new { email, name });
    }
}

public record GoogleTokenRequest(string IdToken);
