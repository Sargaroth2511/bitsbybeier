using Microsoft.EntityFrameworkCore;

namespace bitsbybeier.Data;

/// <summary>
/// Service for initializing and migrating the database.
/// </summary>
public class DatabaseInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseInitializer> _logger;

    /// <summary>
    /// Initializes a new instance of the DatabaseInitializer.
    /// </summary>
    /// <param name="context">Database context.</param>
    /// <param name="logger">Logger instance.</param>
    public DatabaseInitializer(
        ApplicationDbContext context, 
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Applies pending database migrations and initializes the database.
    /// </summary>
    /// <exception cref="Exception">Thrown when database initialization fails.</exception>
    public async Task InitializeAsync()
    {
        try
        {
            // Ensure database is created and migrations are applied
            await _context.Database.MigrateAsync();
            
            _logger.LogInformation("Database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
