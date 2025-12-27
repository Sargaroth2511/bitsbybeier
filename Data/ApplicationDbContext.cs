using Microsoft.EntityFrameworkCore;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add your DbSets here as you create models
    public DbSet<User> Users { get; set; }
    public DbSet<UserImage> UserImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure User entity
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
            
            // Indexes for better performance
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.GoogleId);
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.IsDeleted);
        });
        
        // Configure UserImage entity
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
}
