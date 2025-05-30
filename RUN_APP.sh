#!/bin/bash

echo "🚀 Starting QAgent Application..."
echo "================================"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

# Change to project directory
cd qagent-app/QAgentWeb || {
    echo -e "${RED}❌ Failed to navigate to project directory!${NC}"
    exit 1
}

echo -e "${BLUE}📋 Current directory: $(pwd)${NC}"
echo -e "${YELLOW}🔧 Starting .NET application...${NC}"
echo ""

# Display URLs
echo -e "${CYAN}🌐 Application will be available at:${NC}"
echo -e "${GREEN}   • Local: http://localhost:5174${NC}"
echo -e "${GREEN}   • Codespace: https://[codespace-name]-5174.app.github.dev${NC}"
echo ""
echo -e "${YELLOW}💡 Tip: The port will be automatically forwarded and made public${NC}"
echo -e "${YELLOW}📝 Press Ctrl+C to stop the application${NC}"
echo ""

# Start the application
dotnet run --urls "http://0.0.0.0:5174" 