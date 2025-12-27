using Microsoft.EntityFrameworkCore;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Data;

public class DatabaseInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        ApplicationDbContext context, 
        IConfiguration configuration,
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Ensure database is created and migrations are applied
            await _context.Database.MigrateAsync();

            // Check if any admin user exists
            var adminExists = await _context.Users
                .AnyAsync(u => u.Role == UserRole.Admin && !u.IsDeleted);

            if (!adminExists)
            {
                // Get admin details from configuration
                var adminEmail = _configuration["InitialAdmin:Email"];
                var adminDisplayName = _configuration["InitialAdmin:DisplayName"];

                if (string.IsNullOrEmpty(adminEmail))
                {
                    _logger.LogWarning("No initial admin configured. Skipping admin user creation.");
                    return;
                }

                // Create initial admin user
                var adminUser = new User
                {
                    Email = adminEmail,
                    DisplayName = adminDisplayName ?? "Admin",
                    Role = UserRole.Admin,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Initial admin user created: {Email}", adminEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
