# 🚀 QAgent - GitHub Codespace Deployment Guide

Hướng dẫn deploy và sử dụng QAgent trong GitHub Codespaces.

## 📋 Available Scripts

### 🏠 Local Development (Windows)
```powershell
# Deploy to GitHub
.\DEPLOY_TO_GITHUB.ps1

# Build and run locally
.\REBUILD_AND_RUN_BACKGROUND.ps1
```

### ☁️ GitHub Codespace (Linux)
```bash
# Initial setup and start
./CODESPACE_STARTUP.sh

# Quick start application
./RUN_APP.sh

# Build project
./BUILD_APP.sh
```

## 🚀 Quick Start Guide

### Step 1: Deploy to GitHub
```powershell
# Run from local machine
.\DEPLOY_TO_GITHUB.ps1
```

### Step 2: Create Codespace
1. Visit: https://github.com/phuongt/qagent-ai-test-system
2. Click **"Code"** → **"Codespaces"** → **"Create codespace on main"**
3. Wait for automatic setup (devcontainer will handle everything)

### Step 3: Start Application
```bash
# In Codespace terminal
./CODESPACE_STARTUP.sh
```

### Step 4: Access Application
- **Codespace URL**: `https://[codespace-name]-5174.app.github.dev`
- **Local URL**: `http://localhost:5174` (port forwarded)

## 🔧 Development Workflow

### 1. Make Changes Locally
```powershell
# Edit files in VS Code or any editor
# Test locally with:
.\REBUILD_AND_RUN_BACKGROUND.ps1
```

### 2. Deploy to GitHub
```powershell
# Commit and push changes
.\DEPLOY_TO_GITHUB.ps1
```

### 3. Update Codespace
```bash
# In Codespace, pull latest changes
git pull origin main

# Rebuild if needed
./BUILD_APP.sh

# Restart application
./RUN_APP.sh
```

## 🛠️ Script Details

### `DEPLOY_TO_GITHUB.ps1`
- ✅ Checks git status
- ✅ Commits any uncommitted changes
- ✅ Pushes to GitHub repository
- ✅ Provides Codespace creation links

### `CODESPACE_STARTUP.sh`
- ✅ Checks .NET and Node.js installation
- ✅ Restores NuGet packages
- ✅ Installs npm packages
- ✅ Builds the project
- ✅ Optionally starts the application

### `RUN_APP.sh`
- ✅ Quick application startup
- ✅ Shows access URLs
- ✅ Runs on `http://0.0.0.0:5174`

### `BUILD_APP.sh`
- ✅ Cleans previous builds
- ✅ Restores packages
- ✅ Builds in Release mode

## 🌐 URLs and Access

### Repository
- **GitHub**: https://github.com/phuongt/qagent-ai-test-system
- **Create Codespace**: https://codespaces.new/phuongt/qagent-ai-test-system

### Application URLs (when running)
- **Codespace**: `https://[your-codespace-name]-5174.app.github.dev`
- **Local**: `http://localhost:5174`

## 🎯 Features Included

### 🤖 AI-Powered Test Generation
- Upload UI screenshots
- Automatic test case generation
- ISTQB compliance validation

### 🎨 Modern UI
- Tailwind CSS styling
- Responsive design
- Typing animation effects (Typed.js)

### 🔧 Development Tools
- ASP.NET Core 8.0
- Entity Framework Core
- SQLite database
- Real-time updates

## 🚨 Troubleshooting

### Common Issues

#### 1. Port not accessible
```bash
# Check if port is forwarded
gh codespace ports list

# Forward port manually
gh codespace ports forward 5174:5174 --visibility public
```

#### 2. Build failures
```bash
# Clean and rebuild
./BUILD_APP.sh

# Check .NET version
dotnet --version

# Restore packages manually
cd qagent-app/QAgentWeb
dotnet restore
```

#### 3. Git authentication
```bash
# Configure git in Codespace
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

## 📞 Support

For issues and questions:
1. Check this README first
2. Review script output for error messages
3. Check GitHub repository issues
4. Verify Codespace environment setup

## 🎉 Success Indicators

✅ **Deploy Script**: "Deploy completed successfully!"
✅ **Codespace Setup**: All packages restored and built
✅ **Application**: Accessible via public URL
✅ **Features**: Typing effects working, UI responsive

---
*Generated for QAgent - AI-Powered Test Management System* 