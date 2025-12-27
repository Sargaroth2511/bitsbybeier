# Example: Adding Your First Model

This is a step-by-step example of adding a BlogPost model to demonstrate the code-first workflow.

## Step 1: Create the Model

Create `Domain/Models/BlogPost.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace bitsbybeier.Domain.Models;

public class BlogPost
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Summary { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsPublished { get; set; } = false;
    
    [MaxLength(100)]
    public string? Author { get; set; }
    
    // Navigation properties for future features
    // public List<Comment> Comments { get; set; } = new();
    // public List<Tag> Tags { get; set; } = new();
}
```

## Step 2: Add DbSet to ApplicationDbContext

Edit `Data/ApplicationDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add your DbSets here
    public DbSet<BlogPost> BlogPosts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure BlogPost entity
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Content)
                .IsRequired();
            
            entity.Property(e => e.Summary)
                .HasMaxLength(500);
            
            entity.Property(e => e.Author)
                .HasMaxLength(100);
            
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Index for better query performance
            entity.HasIndex(e => e.IsPublished);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
```

## Step 3: Create Migration

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef migrations add AddBlogPosts
```

This will create a new migration file in the `Migrations` folder.

## Step 4: Review the Migration

Check the generated migration file in `Migrations/`. It should contain:
- `CreateTable` for the `BlogPosts` table
- Proper column definitions
- Indexes

## Step 5: Apply Migration

```bash
dotnet ef database update
```

This will create the `BlogPosts` table in your database.

## Step 6: Verify in Database

```bash
psql -h localhost -U bitsbybeier -d bitsbybeier_web_db

# In psql:
\dt                          # List all tables
\d "BlogPosts"              # Show BlogPosts table structure
SELECT * FROM "BlogPosts";  # Query the table (will be empty)
\q                          # Exit
```

## Step 7: Create a Controller (Optional)

Create `Api/Controllers/BlogController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bitsbybeier.Data;
using bitsbybeier.Domain.Models;

namespace bitsbybeier.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BlogController> _logger;

    public BlogController(ApplicationDbContext context, ILogger<BlogController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogPosts()
    {
        return await _context.BlogPosts
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);

        if (blogPost == null)
        {
            return NotFound();
        }

        return blogPost;
    }

    [HttpPost]
    public async Task<ActionResult<BlogPost>> CreateBlogPost(BlogPost blogPost)
    {
        blogPost.CreatedAt = DateTime.UtcNow;
        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBlogPost), new { id = blogPost.Id }, blogPost);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBlogPost(int id, BlogPost blogPost)
    {
        if (id != blogPost.Id)
        {
            return BadRequest();
        }

        blogPost.UpdatedAt = DateTime.UtcNow;
        _context.Entry(blogPost).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await BlogPostExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBlogPost(int id)
    {
        var blogPost = await _context.BlogPosts.FindAsync(id);
        if (blogPost == null)
        {
            return NotFound();
        }

        _context.BlogPosts.Remove(blogPost);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> BlogPostExists(int id)
    {
        return await _context.BlogPosts.AnyAsync(e => e.Id == id);
    }
}
```

## Step 8: Test Your API

Run your application:
```bash
dotnet run
```

Test with curl or Swagger:
```bash
# Create a blog post
curl -X POST http://localhost:5197/api/blog \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My First Post",
    "content": "This is the content of my first blog post",
    "summary": "A test post",
    "author": "John Doe",
    "isPublished": true
  }'

# Get all blog posts
curl http://localhost:5197/api/blog

# Or visit Swagger UI
# http://localhost:5197/swagger
```

## Common Patterns

### Adding Relationships

```csharp
// One-to-Many: BlogPost to Comments
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    
    // Navigation property
    public List<Comment> Comments { get; set; } = new();
}

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    
    // Foreign key
    public int BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;
}

// In OnModelCreating:
modelBuilder.Entity<Comment>()
    .HasOne(c => c.BlogPost)
    .WithMany(b => b.Comments)
    .HasForeignKey(c => c.BlogPostId)
    .OnDelete(DeleteBehavior.Cascade);
```

### Adding Indexes

```csharp
// In OnModelCreating:
modelBuilder.Entity<BlogPost>()
    .HasIndex(b => b.Title);

// Composite index
modelBuilder.Entity<BlogPost>()
    .HasIndex(b => new { b.Author, b.CreatedAt });

// Unique index
modelBuilder.Entity<BlogPost>()
    .HasIndex(b => b.Title)
    .IsUnique();
```

### Adding Constraints

```csharp
// In OnModelCreating:
modelBuilder.Entity<BlogPost>()
    .Property(b => b.CreatedAt)
    .HasDefaultValueSql("CURRENT_TIMESTAMP");

modelBuilder.Entity<BlogPost>()
    .Property(b => b.IsPublished)
    .HasDefaultValue(false);
```

## Complete Workflow Summary

```bash
# 1. Create model class
nano Domain/Models/YourModel.cs

# 2. Add DbSet to ApplicationDbContext
nano Data/ApplicationDbContext.cs

# 3. Create migration
dotnet ef migrations add AddYourModel

# 4. Review migration
cat Migrations/*_AddYourModel.cs

# 5. Apply migration
dotnet ef database update

# 6. Verify in database
psql -h localhost -U bitsbybeier -d bitsbybeier_web_db -c '\dt'

# 7. Create controller (optional)
nano Api/Controllers/YourModelController.cs

# 8. Run and test
dotnet run
```

That's it! You now have a complete code-first workflow with Entity Framework Core and PostgreSQL.
