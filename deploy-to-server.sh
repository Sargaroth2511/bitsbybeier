#!/bin/bash

# Configuration
SERVER="bitsbybeier.de"
SERVER_USER="johannes"
SERVER_PATH="/var/www/dotnet-app"
LOCAL_PROJECT_PATH="."  # Current directory or specify your project path
SERVICE_NAME="dotnet-web"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Starting deployment to ${SERVER}...${NC}"

# Step 1: Build and publish locally
echo -e "${YELLOW}Building and publishing project...${NC}"
dotnet publish ${LOCAL_PROJECT_PATH} -c Release -o ./publish

if [ $? -ne 0 ]; then
    echo -e "${RED}Build failed!${NC}"
    exit 1
fi

echo -e "${GREEN}Build successful!${NC}"

# Step 2: Stop the service on the server
echo -e "${YELLOW}Stopping service on server...${NC}"
ssh ${SERVER_USER}@${SERVER} "sudo systemctl stop ${SERVICE_NAME}.service"

# Step 3: Backup current version on server
echo -e "${YELLOW}Creating backup...${NC}"
ssh ${SERVER_USER}@${SERVER} "sudo cp -r ${SERVER_PATH} ${SERVER_PATH}.backup.$(date +%Y%m%d-%H%M%S)"

# Step 4: Transfer files to server
echo -e "${YELLOW}Transferring files to server...${NC}"
rsync -avz --delete ./publish/ ${SERVER_USER}@${SERVER}:${SERVER_PATH}/

if [ $? -ne 0 ]; then
    echo -e "${RED}Transfer failed!${NC}"
    exit 1
fi

# Step 5: Set correct permissions
echo -e "${YELLOW}Setting permissions...${NC}"
ssh ${SERVER_USER}@${SERVER} "sudo chown -R www-data:www-data ${SERVER_PATH}"

# Step 6: Start the service
echo -e "${YELLOW}Starting service...${NC}"
ssh ${SERVER_USER}@${SERVER} "sudo systemctl start ${SERVICE_NAME}.service"

# Step 7: Check service status
sleep 2
ssh ${SERVER_USER}@${SERVER} "sudo systemctl is-active ${SERVICE_NAME}.service" > /dev/null 2>&1

if [ $? -eq 0 ]; then
    echo -e "${GREEN}Deployment successful! Service is running.${NC}"
else
    echo -e "${RED}Service failed to start! Check logs with: ssh ${SERVER_USER}@${SERVER} 'sudo journalctl -u ${SERVICE_NAME}.service -n 50'${NC}"
    exit 1
fi

# Cleanup local publish folder
rm -rf ./publish

echo -e "${GREEN}Deployment complete!${NC}"
