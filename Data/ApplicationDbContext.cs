using Microsoft.EntityFrameworkCore;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Data;

/// <summary>
/// Application database context for Entity Framework Core.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext.
    /// </summary>
    /// <param name="options">Database context options.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the Users DbSet.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Gets or sets the UserImages DbSet.
    /// </summary>
    public DbSet<UserImage> UserImages { get; set; }

    /// <summary>
    /// Gets or sets the Content DbSet.
    /// </summary>
    public DbSet<Content> Contents { get; set; }

    /// <summary>
    /// Gets or sets the ContentImages DbSet.
    /// </summary>
    public DbSet<ContentImage> ContentImages { get; set; }

    /// <summary>
    /// Gets or sets the OAuthClients DbSet.
    /// </summary>
    public DbSet<OAuthClient> OAuthClients { get; set; }

    /// <summary>
    /// Gets or sets the OAuthAuthorizationCodes DbSet.
    /// </summary>
    public DbSet<OAuthAuthorizationCode> OAuthAuthorizationCodes { get; set; }

    /// <summary>
    /// Gets or sets the OAuthRefreshTokens DbSet.
    /// </summary>
    public DbSet<OAuthRefreshToken> OAuthRefreshTokens { get; set; }

    /// <summary>
    /// Configures the database model and relationships.
    /// </summary>
    /// <param name="modelBuilder">Model builder for entity configuration.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureUser(modelBuilder);
        ConfigureUserImage(modelBuilder);
        ConfigureContent(modelBuilder);
        ConfigureContentImage(modelBuilder);
        ConfigureOAuthClient(modelBuilder);
        ConfigureOAuthAuthorizationCode(modelBuilder);
        ConfigureOAuthRefreshToken(modelBuilder);
    }

    /// <summary>
    /// Configures the User entity with properties, indexes, and relationships.
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration.</param>
    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255);
            
            entity.Property(e => e.Role)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(UserRole.User);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
            
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
            
            // Relationship with UserImage
            entity.HasOne(e => e.ProfileImage)
                .WithMany()
                .HasForeignKey(e => e.ProfileImageId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Indexes for better query performance
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.GoogleId);
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.IsDeleted);
        });
    }

    /// <summary>
    /// Configures the UserImage entity with properties and constraints.
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration.</param>
    private static void ConfigureUserImage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ImageData)
                .IsRequired();
            
            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.FileName)
                .HasMaxLength(255);
            
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    /// <summary>
    /// Configures the Content entity with properties, indexes, and relationships.
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration.</param>
    private static void ConfigureContent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Content>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Author)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.Subtitle)
                .HasMaxLength(1000);
            
            entity.Property(e => e.ContentText)
                .IsRequired();
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.Active)
                .HasDefaultValue(true);
            
            entity.Property(e => e.Draft)
                .HasDefaultValue(true);
            
            // Relationship with ContentImage
            entity.HasMany(e => e.Images)
                .WithOne(i => i.Content)
                .HasForeignKey(i => i.ContentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for better query performance
            entity.HasIndex(e => e.Draft);
            entity.HasIndex(e => e.Active);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    /// <summary>
    /// Configures the ContentImage entity with properties and constraints.
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration.</param>
    private static void ConfigureContentImage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ImageData)
                .IsRequired();
            
            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.FileName)
                .HasMaxLength(255);
            
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    /// <summary>
    /// Configures the OAuthClient entity with properties, indexes, and relationships.
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration.</param>
    private static void ConfigureOAuthClient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OAuthClient>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ClientId)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.ClientSecret)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.ClientName)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.RedirectUris)
                .IsRequired();
            
            entity.Property(e => e.AllowedScopes)
                .IsRequired();
            
            entity.Property(e => e.Active)
                .HasDefaultValue(true);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Relationship with User
            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for better query performance
            entity.HasIndex(e => e.ClientId).IsUnique();
            entity.HasIndex(e => e.Active);
        });
    }

    /// <summary>
    /// Configures the OAuthAuthorizationCode entity with properties and indexes.
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration.</param>
    private static void ConfigureOAuthAuthorizationCode(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OAuthAuthorizationCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.ClientId)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.RedirectUri)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.Scope)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.CodeChallenge)
                .HasMaxLength(100);
            
            entity.Property(e => e.CodeChallengeMethod)
                .HasMaxLength(10);
            
            entity.Property(e => e.Used)
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Relationship with User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for better query performance
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.Used);
        });
    }

    /// <summary>
    /// Configures the OAuthRefreshToken entity with properties and indexes.
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuration.</param>
    private static void ConfigureOAuthRefreshToken(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OAuthRefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.ClientId)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Scope)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.Revoked)
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Relationship with User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for better query performance
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.Revoked);
            entity.HasIndex(e => e.ExpiresAt);
        });
    }
}
