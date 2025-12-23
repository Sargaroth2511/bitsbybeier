#!/bin/bash
set -euo pipefail

# Configuration
SERVER="bitsbybeier.de"
SERVER_USER="johannes"
SERVER_PORT="2500"
SERVER_PATH="/var/www/dotnet-app"
LOCAL_PROJECT_PATH="."  # Current directory or specify your project path
SERVICE_NAME="dotnet-web"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Starting deployment to ${SERVER}...${NC}"

# Step 0: Clean previous publish output
rm -rf ./publish

# Step 1: Build Angular (production)
echo -e "${YELLOW}Building Angular frontend (production)...${NC}"
if [ -d "ClientApp" ]; then
    pushd ClientApp >/dev/null
    npm ci
    echo -e "${YELLOW}Fixing npm vulnerabilities...${NC}"
    npm audit fix --force 2>/dev/null || echo "Some vulnerabilities could not be auto-fixed"
    npm run build -- --configuration production
    popd >/dev/null
    echo -e "${GREEN}Angular build complete.${NC}"
else
    echo -e "${YELLOW}No ClientApp directory found, skipping Angular build.${NC}"
fi

# Step 2: Build and publish backend
echo -e "${YELLOW}Building and publishing backend...${NC}"
dotnet publish ${LOCAL_PROJECT_PATH} -c Release -o ./publish
echo -e "${GREEN}Backend publish complete.${NC}"

# Step 3: Stop the service on the server (if it exists)
echo -e "${YELLOW}Stopping service on server...${NC}"
ssh -p ${SERVER_PORT} ${SERVER_USER}@${SERVER} "sudo systemctl stop ${SERVICE_NAME}.service 2>/dev/null || echo 'Service not running or does not exist yet'"

# Step 4: Backup current version on server (if it exists)
echo -e "${YELLOW}Creating backup...${NC}"
ssh -p ${SERVER_PORT} ${SERVER_USER}@${SERVER} '[ -d '"${SERVER_PATH}"' ] && sudo cp -r '"${SERVER_PATH}"' '"${SERVER_PATH}"'.backup.$(date +%Y%m%d-%H%M%S) || echo "No existing deployment to backup"'

# Step 5: Ensure directory exists
ssh -p ${SERVER_PORT} ${SERVER_USER}@${SERVER} "sudo mkdir -p ${SERVER_PATH}"

# Step 6: Transfer files to server (to temp location first)
echo -e "${YELLOW}Transferring files to server...${NC}"
TEMP_PATH="/home/${SERVER_USER}/dotnet-app-temp"
ssh -p ${SERVER_PORT} ${SERVER_USER}@${SERVER} "rm -rf ${TEMP_PATH} && mkdir -p ${TEMP_PATH}"
rsync -avz --delete -e "ssh -p ${SERVER_PORT}" ./publish/ ${SERVER_USER}@${SERVER}:${TEMP_PATH}/
ssh -p ${SERVER_PORT} ${SERVER_USER}@${SERVER} "sudo rm -rf ${SERVER_PATH}/* && sudo cp -r ${TEMP_PATH}/* ${SERVER_PATH}/ && rm -rf ${TEMP_PATH}"
echo -e "${GREEN}Transfer complete.${NC}"

# Step 7: Set correct permissions
echo -e "${YELLOW}Setting permissions...${NC}"
ssh -p ${SERVER_PORT} ${SERVER_USER}@${SERVER} "sudo chown -R www-data:www-data ${SERVER_PATH}"

# Step 8: Start the service
echo -e "${YELLOW}Starting service...${NC}"
ssh -p ${SERVER_PORT} ${SERVER_USER}@${SERVER} "sudo systemctl start ${SERVICE_NAME}.service 2>/dev/null || echo 'Service file not found - you need to create /etc/systemd/system/${SERVICE_NAME}.service first'"

# Step 9: Check service status
sleep 2
if ssh -p ${SERVER_PORT} ${SERVER_USER}@${SERVER} "sudo systemctl is-active ${SERVICE_NAME}.service" > /dev/null 2>&1; then
    echo -e "${GREEN}Deployment successful! Service is running.${NC}"
else
    echo -e "${YELLOW}Files deployed, but service not running.${NC}"
    echo -e "${YELLOW}Check if service file exists: /etc/systemd/system/${SERVICE_NAME}.service${NC}"
    echo -e "${YELLOW}Or check logs: ssh -p ${SERVER_PORT} ${SERVER_USER}@${SERVER} 'sudo journalctl -u ${SERVICE_NAME}.service -n 50'${NC}"
fi

# Cleanup local publish folder
rm -rf ./publish

echo -e "${GREEN}Deployment complete!${NC}"
