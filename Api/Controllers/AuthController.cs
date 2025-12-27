using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bitsbybeier.Api.Services;
using bitsbybeier.Data;
using bitsbybeier.Domain.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Google.Apis.Auth;

namespace bitsbybeier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IJwtTokenService jwtTokenService, 
        IConfiguration configuration,
        ApplicationDbContext context,
        ILogger<AuthController> logger)
    {
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _context = context;
        _logger = logger;
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

            // Look up or create user in database
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == payload.Email && !u.IsDeleted);

            if (user == null)
            {
                // Auto-create new user with default User role
                user = new User
                {
                    Email = payload.Email,
                    DisplayName = payload.Name,
                    GoogleId = payload.Subject,
                    Role = UserRole.User,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("New user created: {Email} with role {Role}", user.Email, user.Role);
            }
            else if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account is deactivated" });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Create claims with user role
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email)
            };

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(claims);

            return Ok(new 
            { 
                token, 
                email = user.Email, 
                name = user.DisplayName,
                role = user.Role.ToString(),
                userId = user.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error");
            return Unauthorized(new { message = "Authentication failed" });
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
