#!/bin/bash
# Run this on the server to check if Stability AI key is loaded

echo "Checking .env file contents..."
sudo cat /var/www/dotnet-app/.env | grep -i stability

echo ""
echo "Checking if service can see the variable..."
sudo systemctl show dotnet-web.service -p Environment | grep -i stability

echo ""
echo "Restarting service to pick up new environment variables..."
sudo systemctl restart dotnet-web.service

echo ""
echo "Waiting for service to start..."
sleep 2

echo ""
echo "Service status:"
sudo systemctl status dotnet-web.service | grep -A 5 "Active:"
