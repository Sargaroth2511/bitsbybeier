#!/bin/bash

# Database Migration and Deployment Script
# This script applies pending Entity Framework Core migrations to the database

set -e  # Exit on any error

echo "=================================="
echo "Database Migration Deployment"
echo "=================================="

# Check if environment is specified
if [ -z "$1" ]; then
    echo "Usage: ./deploy-database.sh [environment]"
    echo "Example: ./deploy-database.sh Production"
    exit 1
fi

ENVIRONMENT=$1
PROJECT_DIR="/home/sargaroth/bitsbybeier"

echo "Environment: $ENVIRONMENT"
echo "Project Directory: $PROJECT_DIR"

cd $PROJECT_DIR

# For Production, we need to ensure environment variables are set
if [ "$ENVIRONMENT" == "Production" ]; then
    echo ""
    echo "Checking required environment variables..."
    
    required_vars=("DB_HOST" "DB_PORT" "DB_USER" "DB_PASSWORD")
    missing_vars=()
    
    for var in "${required_vars[@]}"; do
        if [ -z "${!var}" ]; then
            missing_vars+=("$var")
        fi
    done
    
    if [ ${#missing_vars[@]} -gt 0 ]; then
        echo "ERROR: Missing required environment variables:"
        for var in "${missing_vars[@]}"; do
            echo "  - $var"
        done
        echo ""
        echo "Please set these in your server environment or /etc/environment"
        exit 1
    fi
    
    echo "✓ All required environment variables are set"
fi

# Apply migrations
echo ""
echo "Applying database migrations..."
dotnet ef database update --project $PROJECT_DIR

if [ $? -eq 0 ]; then
    echo ""
    echo "✓ Database migrations applied successfully!"
    echo "=================================="
else
    echo ""
    echo "✗ Migration failed!"
    exit 1
fi
