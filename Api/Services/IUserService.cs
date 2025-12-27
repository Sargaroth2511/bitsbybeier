using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Services;

/// <summary>
/// Service for managing user operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Finds a user by email address.
    /// </summary>
    /// <param name="email">Email address to search for.</param>
    /// <returns>User if found, null otherwise.</returns>
    Task<User?> FindByEmailAsync(string email);

    /// <summary>
    /// Creates a new user with the specified details.
    /// </summary>
    /// <param name="email">User's email address.</param>
    /// <param name="displayName">User's display name.</param>
    /// <param name="googleId">Google account identifier.</param>
    /// <returns>The newly created user.</returns>
    Task<User> CreateUserAsync(string email, string displayName, string googleId);

    /// <summary>
    /// Updates the user's last login timestamp.
    /// </summary>
    /// <param name="user">User to update.</param>
    Task UpdateLastLoginAsync(User user);
}
