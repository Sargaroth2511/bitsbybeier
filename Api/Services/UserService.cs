using Microsoft.EntityFrameworkCore;
using bitsbybeier.Data;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service implementation for user management operations.
/// </summary>
public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Initializes a new instance of the UserService.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="logger">Logger instance.</param>
    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Finds a user by email address, excluding deleted users.
    /// </summary>
    /// <param name="email">Email address to search for.</param>
    /// <returns>User if found and not deleted, null otherwise.</returns>
    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
    }

    /// <summary>
    /// Creates a new user with default User role and active status.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="displayName">User's display name.</param>
    /// <param name="googleId">Google account identifier.</param>
    /// <returns>The newly created user.</returns>
    public async Task<User> CreateUserAsync(string email, string displayName, string googleId)
    {
        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            GoogleId = googleId,
            Role = UserRole.User,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New user created: {Email} with role {Role}", user.Email, user.Role);

        return user;
    }

    /// <summary>
    /// Updates the user's last login timestamp to current UTC time.
    /// </summary>
    /// <param name="user">User to update.</param>
    public async Task UpdateLastLoginAsync(User user)
    {
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
