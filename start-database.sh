#!/bin/bash

# Start PostgreSQL Database Script
# This script starts the PostgreSQL database service for the bitsbybeier project

echo "Starting PostgreSQL database..."

# Start PostgreSQL service
sudo service postgresql start

# Check if the service started successfully
if sudo service postgresql status | grep -q "online"; then
    echo "✓ PostgreSQL is now running"
    
    # Display connection info
    echo ""
    echo "Database Information:"
    echo "  - Host: localhost"
    echo "  - Port: 5432"
    echo "  - Database: bitsbybeier_web_db"
    echo "  - User: bitsbybeier"
    echo ""
    echo "You can now run 'dotnet run' to start the application"
else
    echo "✗ Failed to start PostgreSQL"
    exit 1
fi
