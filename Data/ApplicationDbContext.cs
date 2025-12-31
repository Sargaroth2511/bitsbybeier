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
}
