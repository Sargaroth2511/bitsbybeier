using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using bitsbybeier.Api.Configuration;
using bitsbybeier.Api.Models;
using bitsbybeier.Api.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Google.Apis.Auth;

namespace bitsbybeier.Api.Controllers;

/// <summary>
/// Controller for handling user authentication and authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserService _userService;
    private readonly GoogleAuthOptions _googleAuthOptions;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the AuthController.
    /// </summary>
    /// <param name="jwtTokenService">JWT token generation service.</param>
    /// <param name="userService">User management service.</param>
    /// <param name="googleAuthOptions">Google authentication configuration.</param>
    /// <param name="logger">Logger instance.</param>
    public AuthController(
        IJwtTokenService jwtTokenService,
        IUserService userService,
        IOptions<GoogleAuthOptions> googleAuthOptions,
        ILogger<AuthController> logger)
    {
        _jwtTokenService = jwtTokenService;
        _userService = userService;
        _googleAuthOptions = googleAuthOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user using Google OAuth ID token.
    /// </summary>
    /// <param name="request">Request containing the Google ID token.</param>
    /// <returns>Authentication response with JWT token and user information.</returns>
    /// <response code="200">Returns the JWT token and user information.</response>
    /// <response code="401">If authentication fails or token is invalid.</response>
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleTokenRequest request)
    {
        try
        {
            // Verify the Google ID token
            var payload = await ValidateGoogleTokenAsync(request.IdToken);
            if (payload == null)
            {
                return Unauthorized(new ErrorResponse { Message = "Invalid Google token" });
            }

            // Find or create user
            var user = await _userService.FindByEmailAsync(payload.Email);
            
            if (user == null)
            {
                user = await _userService.CreateUserAsync(payload.Email, payload.Name, payload.Subject);
            }
            else if (!user.IsActive)
            {
                return Unauthorized(new ErrorResponse { Message = "Account is deactivated" });
            }

            // Update last login timestamp
            await _userService.UpdateLastLoginAsync(user);

            // Generate JWT token with user claims
            var claims = CreateUserClaims(user);
            var token = _jwtTokenService.GenerateToken(claims);

            return Ok(new AuthenticationResponse
            {
                Token = token,
                Email = user.Email,
                Name = user.DisplayName,
                Role = user.Role.ToString(),
                UserId = user.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error for Google login");
            return Unauthorized(new ErrorResponse { Message = "Authentication failed" });
        }
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    /// <returns>User information from the JWT token claims.</returns>
    /// <response code="200">Returns the user information.</response>
    /// <response code="401">If user is not authenticated.</response>
    [HttpGet("user")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
        {
            return Unauthorized(new ErrorResponse { Message = "Invalid user claims" });
        }

        return Ok(new UserResponse { Email = email, Name = name });
    }

    /// <summary>
    /// Validates a Google ID token and returns the payload.
    /// </summary>
    /// <param name="idToken">Google ID token to validate.</param>
    /// <returns>Token payload if valid, null otherwise.</returns>
    private async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { _googleAuthOptions.ClientId }
            };

            return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate Google token");
            return null;
        }
    }

    /// <summary>
    /// Creates a collection of claims for a user to be included in the JWT token.
    /// </summary>
    /// <param name="user">User for whom to create claims.</param>
    /// <returns>Collection of claims representing the user.</returns>
    private static IEnumerable<Claim> CreateUserClaims(Domain.Models.User user)
    {
        return new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email)
        };
    }
}
