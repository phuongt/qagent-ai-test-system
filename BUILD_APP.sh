#!/bin/bash

echo "🔨 Building QAgent Application..."
echo "================================="

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m'

# Change to project directory
cd qagent-app/QAgentWeb || {
    echo -e "${RED}❌ Failed to navigate to project directory!${NC}"
    exit 1
}

echo -e "${BLUE}📂 Current directory: $(pwd)${NC}"
echo ""

# Clean previous builds
echo -e "${YELLOW}🧹 Cleaning previous builds...${NC}"
dotnet clean
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Clean completed${NC}"
else
    echo -e "${RED}❌ Clean failed${NC}"
fi
echo ""

# Restore packages
echo -e "${YELLOW}📦 Restoring NuGet packages...${NC}"
dotnet restore
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Packages restored${NC}"
else
    echo -e "${RED}❌ Package restore failed${NC}"
    exit 1
fi
echo ""

# Build the project
echo -e "${YELLOW}🔨 Building project...${NC}"
dotnet build --configuration Release
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Build completed successfully!${NC}"
    echo ""
    echo -e "${BLUE}📋 Next steps:${NC}"
    echo -e "   • Run: ${YELLOW}./RUN_APP.sh${NC} to start the application"
    echo -e "   • Run: ${YELLOW}dotnet run${NC} to start in development mode"
    echo -e "   • Run: ${YELLOW}dotnet test${NC} to run tests (if available)"
else
    echo -e "${RED}❌ Build failed!${NC}"
    exit 1
fi 