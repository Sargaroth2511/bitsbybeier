#!/bin/bash
# Run this on your production server to add Stability AI API key

echo "Adding Stability AI API key to systemd service..."
echo ""

# Edit the service file
sudo systemctl edit dotnet-web.service --full

# After editing, reload and restart
sudo systemctl daemon-reload
sudo systemctl restart dotnet-web.service

# Verify it's running
sudo systemctl status dotnet-web.service
