# Database Setup and Configuration

## Overview
This project uses PostgreSQL with Entity Framework Core in a code-first approach. Connection strings are managed securely using:
- **Development**: User Secrets
- **Production**: Environment Variables

## Local Development Setup

### 1. PostgreSQL Installation (Completed)
PostgreSQL 16 has been installed and configured with:
- Database: `bitsbybeier_web_db`
- User: `bitsbybeier`
- Password: `your_dev_password`

### 2. Connection String Configuration

#### For Development (Local)
The connection string is stored in User Secrets for security:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=bitsbybeier_web_db;Username=bitsbybeier;Password=your_dev_password"
```

To view your secrets:
```bash
dotnet user-secrets list
```

#### For Production (Server)
Set the following environment variables on your server:

```bash
export DB_HOST=your-db-host
export DB_PORT=5432
export DB_USER=your-db-username
export DB_PASSWORD=your-secure-password
```

Add these to `/etc/environment` for persistence across reboots:
```
DB_HOST=your-db-host
DB_PORT=5432
DB_USER=your-db-username
DB_PASSWORD=your-secure-password
```

The `appsettings.json` will automatically substitute these variables:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=${DB_HOST};Port=${DB_PORT};Database=bitsbybeier_web_db;Username=${DB_USER};Password=${DB_PASSWORD};Ssl Mode=Prefer;Trust Server Certificate=true"
}
```

## Entity Framework Core Commands

### Create a New Migration
```bash
dotnet ef migrations add MigrationName
```

### Apply Migrations (Update Database)
```bash
# Local Development
dotnet ef database update

# Production (after setting environment variables)
./deploy-database.sh Production
```

### Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove
```

### View Migration History
```bash
dotnet ef migrations list
```

### Generate SQL Script
```bash
dotnet ef migrations script
```

## Code-First Development Workflow

1. **Create/Modify Models** in the `Domain/Models` folder
2. **Update DbContext** in `Data/ApplicationDbContext.cs` to add DbSets
3. **Create Migration**:
   ```bash
   dotnet ef migrations add YourMigrationName
   ```
4. **Review Migration** in `Migrations` folder
5. **Apply Migration**:
   ```bash
   dotnet ef database update
   ```

### Example: Creating Your First Model

1. Create a model in `Domain/Models/BlogPost.cs`:
```csharp
namespace bitsbybeier.Domain.Models;

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

2. Add DbSet to `Data/ApplicationDbContext.cs`:
```csharp
public DbSet<BlogPost> BlogPosts { get; set; }
```

3. Create and apply migration:
```bash
dotnet ef migrations add AddBlogPosts
dotnet ef database update
```

## Production Deployment

### Initial Setup
1. Install PostgreSQL on your server
2. Create the database: `bitsbybeier_web_db`
3. Set environment variables (DB_HOST, DB_PORT, DB_USER, DB_PASSWORD)
4. Deploy your application code

### Deploying Database Changes
```bash
# SSH to your server
ssh user@your-server

# Navigate to project directory
cd /path/to/bitsbybeier

# Ensure environment variables are set
echo $DB_HOST  # Should show your database host

# Run deployment script
./deploy-database.sh Production
```

## Troubleshooting

### Connection Issues
```bash
# Test PostgreSQL connection
psql -h localhost -U bitsbybeier -d bitsbybeier_web_db

# Check PostgreSQL service status
sudo service postgresql status

# View PostgreSQL logs
sudo tail -f /var/log/postgresql/postgresql-16-main.log
```

### EF Core Tools
If you get "No executable found matching command 'dotnet-ef'":
```bash
dotnet tool install --global dotnet-ef
```

Update EF Core tools:
```bash
dotnet tool update --global dotnet-ef
```

## Security Notes

- ✓ Never commit connection strings to source control
- ✓ Use User Secrets for local development
- ✓ Use Environment Variables for production
- ✓ Use strong passwords for production databases
- ✓ Enable SSL/TLS for production database connections
- ✓ Restrict database user permissions to only what's needed

## Backup Recommendations

### Local Backup
```bash
pg_dump -U bitsbybeier -d bitsbybeier_web_db > backup_$(date +%Y%m%d).sql
```

### Production Backup
Set up automated backups using cron:
```bash
0 2 * * * pg_dump -h $DB_HOST -U $DB_USER -d bitsbybeier_web_db | gzip > /backups/db_backup_$(date +\%Y\%m\%d_\%H\%M\%S).sql.gz
```
