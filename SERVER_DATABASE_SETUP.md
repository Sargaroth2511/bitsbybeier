# Production Server Database Configuration

## Environment Variables Setup

For the production server, you need to set the following environment variables. These will be used by the application to connect to the PostgreSQL database.

### Required Environment Variables

```bash
# Database Connection
DB_HOST=localhost              # or your database server IP/hostname
DB_PORT=5432                  # PostgreSQL default port
DB_USER=bitsbybeier           # Database user
DB_PASSWORD=your_secure_password_here

# JWT Configuration
JWT_SECRET=your-production-jwt-secret-at-least-32-characters-long

# Google OAuth (if using)
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret
```

### Setting Environment Variables

#### Option 1: Persistent Environment Variables (Recommended)

Edit `/etc/environment` (requires sudo):
```bash
sudo nano /etc/environment
```

Add these lines:
```
DB_HOST="localhost"
DB_PORT="5432"
DB_USER="bitsbybeier"
DB_PASSWORD="your_secure_password_here"
JWT_SECRET="your-production-jwt-secret-at-least-32-characters-long"
GOOGLE_CLIENT_ID="your-google-client-id"
GOOGLE_CLIENT_SECRET="your-google-client-secret"
```

Save and reload:
```bash
source /etc/environment
```

#### Option 2: User Profile (Alternative)

Edit `~/.bashrc` or `~/.profile`:
```bash
nano ~/.bashrc
```

Add at the end:
```bash
export DB_HOST="localhost"
export DB_PORT="5432"
export DB_USER="bitsbybeier"
export DB_PASSWORD="your_secure_password_here"
export JWT_SECRET="your-production-jwt-secret-at-least-32-characters-long"
export GOOGLE_CLIENT_ID="your-google-client-id"
export GOOGLE_CLIENT_SECRET="your-google-client-secret"
```

Reload:
```bash
source ~/.bashrc
```

#### Option 3: Systemd Service File (For systemd services)

If running as a systemd service, edit your service file:
```bash
sudo nano /etc/systemd/system/bitsbybeier.service
```

Add in the `[Service]` section:
```ini
[Service]
Environment="DB_HOST=localhost"
Environment="DB_PORT=5432"
Environment="DB_USER=bitsbybeier"
Environment="DB_PASSWORD=your_secure_password_here"
Environment="JWT_SECRET=your-production-jwt-secret-at-least-32-characters-long"
Environment="GOOGLE_CLIENT_ID=your-google-client-id"
Environment="GOOGLE_CLIENT_SECRET=your-google-client-secret"
```

Reload systemd:
```bash
sudo systemctl daemon-reload
sudo systemctl restart bitsbybeier
```

### Verify Environment Variables

```bash
# Check if variables are set
echo $DB_HOST
echo $DB_USER
# Don't echo passwords in production for security!

# Or check all at once (be careful with passwords)
printenv | grep DB_
printenv | grep JWT_
printenv | grep GOOGLE_
```

## Database Setup on Server

### 1. Install PostgreSQL
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
```

### 2. Start PostgreSQL
```bash
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

### 3. Create Database and User
```bash
# Switch to postgres user
sudo -u postgres psql

# In PostgreSQL console:
CREATE DATABASE bitsbybeier_web_db;
CREATE USER bitsbybeier WITH PASSWORD 'your_secure_password_here';
GRANT ALL PRIVILEGES ON DATABASE bitsbybeier_web_db TO bitsbybeier;
\c bitsbybeier_web_db
GRANT ALL ON SCHEMA public TO bitsbybeier;
\q
```

### 4. Deploy Application
```bash
# Copy your application files to server
scp -r /home/sargaroth/bitsbybeier user@server:/path/to/deploy/

# Or use git
cd /path/to/deploy
git pull origin main
```

### 5. Run Database Migrations
```bash
cd /path/to/bitsbybeier
./deploy-database.sh Production
```

## Security Checklist

- [ ] Use strong, unique passwords for database users
- [ ] Set environment variables securely (not in source control)
- [ ] Enable SSL/TLS for database connections in production
- [ ] Restrict PostgreSQL to listen only on localhost (if app and DB are on same server)
- [ ] Configure firewall rules to restrict database port access
- [ ] Regularly backup the database
- [ ] Keep PostgreSQL and all packages up to date
- [ ] Use a different database user password than development
- [ ] Generate a strong, unique JWT secret for production
- [ ] Review and audit database user permissions

## PostgreSQL Configuration (Optional)

Edit `/etc/postgresql/16/main/postgresql.conf` for production optimizations:
```bash
sudo nano /etc/postgresql/16/main/postgresql.conf
```

Key settings to consider:
```
max_connections = 100           # Adjust based on your needs
shared_buffers = 256MB          # 25% of RAM for dedicated DB server
effective_cache_size = 1GB      # 50% of RAM
maintenance_work_mem = 128MB
checkpoint_completion_target = 0.9
wal_buffers = 16MB
default_statistics_target = 100
random_page_cost = 1.1          # For SSD storage
effective_io_concurrency = 200  # For SSD storage
work_mem = 2621kB
```

Restart PostgreSQL after changes:
```bash
sudo systemctl restart postgresql
```

## Monitoring

### Check Database Size
```bash
sudo -u postgres psql -d bitsbybeier_web_db -c "SELECT pg_size_pretty(pg_database_size('bitsbybeier_web_db'));"
```

### Check Active Connections
```bash
sudo -u postgres psql -c "SELECT count(*) FROM pg_stat_activity WHERE datname = 'bitsbybeier_web_db';"
```

### View Recent Queries
```bash
sudo -u postgres psql -d bitsbybeier_web_db -c "SELECT pid, usename, application_name, state, query FROM pg_stat_activity WHERE datname = 'bitsbybeier_web_db';"
```
