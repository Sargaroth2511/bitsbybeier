#!/bin/bash
# Quick Entity Framework Core Commands Reference

# Note: Make sure dotnet-ef tools are in PATH
export PATH="$PATH:$HOME/.dotnet/tools"

echo "Entity Framework Core - Quick Commands"
echo "======================================"
echo ""
echo "Current directory: $(pwd)"
echo ""

# Function to show usage
show_usage() {
    echo "Usage: ./ef-helper.sh [command]"
    echo ""
    echo "Available commands:"
    echo "  migration-add <name>    - Create a new migration"
    echo "  migration-remove        - Remove the last migration (not yet applied)"
    echo "  migration-list          - List all migrations"
    echo "  database-update         - Apply pending migrations to database"
    echo "  database-drop           - Drop the database"
    echo "  sql-script              - Generate SQL script from migrations"
    echo "  dbcontext-info          - Show DbContext information"
    echo "  dbcontext-list          - List all DbContext types"
    echo ""
    echo "Examples:"
    echo "  ./ef-helper.sh migration-add AddBlogPosts"
    echo "  ./ef-helper.sh database-update"
}

# Check if command is provided
if [ -z "$1" ]; then
    show_usage
    exit 1
fi

COMMAND=$1

case $COMMAND in
    migration-add)
        if [ -z "$2" ]; then
            echo "Error: Migration name is required"
            echo "Usage: ./ef-helper.sh migration-add <MigrationName>"
            exit 1
        fi
        echo "Creating migration: $2"
        dotnet ef migrations add "$2"
        ;;
    
    migration-remove)
        echo "Removing last migration..."
        dotnet ef migrations remove
        ;;
    
    migration-list)
        echo "Listing all migrations..."
        dotnet ef migrations list
        ;;
    
    database-update)
        echo "Applying pending migrations to database..."
        dotnet ef database update
        ;;
    
    database-drop)
        echo "WARNING: This will delete all data in the database!"
        read -p "Are you sure? (yes/no): " confirm
        if [ "$confirm" == "yes" ]; then
            dotnet ef database drop --force
            echo "Database dropped successfully"
        else
            echo "Operation cancelled"
        fi
        ;;
    
    sql-script)
        OUTPUT_FILE="migration_script_$(date +%Y%m%d_%H%M%S).sql"
        echo "Generating SQL script: $OUTPUT_FILE"
        dotnet ef migrations script > "$OUTPUT_FILE"
        echo "SQL script saved to: $OUTPUT_FILE"
        ;;
    
    dbcontext-info)
        echo "DbContext information..."
        dotnet ef dbcontext info
        ;;
    
    dbcontext-list)
        echo "Listing all DbContext types..."
        dotnet ef dbcontext list
        ;;
    
    *)
        echo "Error: Unknown command '$COMMAND'"
        echo ""
        show_usage
        exit 1
        ;;
esac
