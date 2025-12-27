namespace bitsbybeier.Domain.Models;

/// <summary>
/// Defines user roles in the system for authorization purposes.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standard user with basic permissions.
    /// </summary>
    User = 0,

    /// <summary>
    /// Administrator with full system access.
    /// </summary>
    Admin = 1
}
