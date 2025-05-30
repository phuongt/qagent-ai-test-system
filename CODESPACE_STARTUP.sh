#!/bin/bash

echo "🚀 QAgent - Codespace Startup Script"
echo "===================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}📋 $1${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${CYAN}ℹ️  $1${NC}"
}

# Change to project directory
print_status "Navigating to project directory..."
cd qagent-app/QAgentWeb || {
    print_error "Failed to navigate to qagent-app/QAgentWeb directory!"
    exit 1
}
print_success "Changed to project directory"

# Check if .NET is installed
print_status "Checking .NET installation..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    print_success ".NET is installed (version: $DOTNET_VERSION)"
else
    print_error ".NET is not installed!"
    exit 1
fi

# Check if Node.js is installed
print_status "Checking Node.js installation..."
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    print_success "Node.js is installed (version: $NODE_VERSION)"
else
    print_warning "Node.js is not installed, some features may not work"
fi

# Restore .NET packages
print_status "Restoring .NET packages..."
dotnet restore
if [ $? -eq 0 ]; then
    print_success "NuGet packages restored successfully"
else
    print_error "Failed to restore NuGet packages"
    exit 1
fi

# Install npm packages if package.json exists
if [ -f "package.json" ]; then
    print_status "Installing npm packages..."
    npm install
    if [ $? -eq 0 ]; then
        print_success "npm packages installed successfully"
    else
        print_warning "Failed to install npm packages, continuing anyway..."
    fi
fi

# Build the project
print_status "Building the project..."
dotnet build
if [ $? -eq 0 ]; then
    print_success "Project built successfully"
else
    print_error "Failed to build project"
    exit 1
fi

# Display startup information
echo ""
print_info "🎉 Setup completed successfully!"
echo ""
print_info "📋 To start the application:"
print_info "   dotnet run --urls \"http://0.0.0.0:5174\""
echo ""
print_info "🌐 The application will be available at:"
print_info "   https://[codespace-name]-5174.app.github.dev"
echo ""
print_info "🔧 Available commands:"
print_info "   ./CODESPACE_STARTUP.sh     - Run this setup script"
print_info "   ./RUN_APP.sh              - Start the application"
print_info "   ./BUILD_APP.sh            - Build the application"
echo ""

# Ask if user wants to start the app now
read -p "🚀 Do you want to start the application now? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_status "Starting the application..."
    dotnet run --urls "http://0.0.0.0:5174"
else
    print_info "You can start the application later with: dotnet run --urls \"http://0.0.0.0:5174\""
fi 