#!/usr/bin/env pwsh

Write-Host "🚀 QAgent - GitHub Deploy Script" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Function to check if command exists
function Test-Command {
    param([string]$Command)
    $null = Get-Command $Command -ErrorAction SilentlyContinue
    return $?
}

# Check prerequisites
Write-Host "🔍 Checking prerequisites..." -ForegroundColor Yellow

if (-not (Test-Command "git")) {
    Write-Host "❌ Git is not installed!" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path ".git")) {
    Write-Host "❌ Not a git repository!" -ForegroundColor Red
    exit 1
}

# Check git status
Write-Host "📊 Checking git status..." -ForegroundColor Yellow
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "📝 Uncommitted changes detected. Adding and committing..." -ForegroundColor Yellow
    
    # Add all changes
    git add .
    
    # Get commit message from user or use default
    $commitMessage = Read-Host "Enter commit message (press Enter for default)"
    if ([string]::IsNullOrWhiteSpace($commitMessage)) {
        $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        $commitMessage = "Update QAgent system - $timestamp"
    }
    
    # Commit changes
    git commit -m "$commitMessage"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to commit changes!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Changes committed successfully!" -ForegroundColor Green
} else {
    Write-Host "✅ No uncommitted changes detected." -ForegroundColor Green
}

# Check if remote origin exists
$remoteUrl = git remote get-url origin 2>$null
if (-not $remoteUrl) {
    Write-Host "❌ No remote 'origin' found!" -ForegroundColor Red
    Write-Host "Please run: git remote add origin https://github.com/phuongt/qagent-ai-test-system.git" -ForegroundColor Yellow
    exit 1
}

Write-Host "🔗 Remote URL: $remoteUrl" -ForegroundColor Cyan

# Push to GitHub
Write-Host "📤 Pushing to GitHub..." -ForegroundColor Yellow
git push origin main
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to push to GitHub!" -ForegroundColor Red
    Write-Host "You may need to authenticate with GitHub first." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Successfully deployed to GitHub!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 Repository URL: https://github.com/phuongt/qagent-ai-test-system" -ForegroundColor Cyan
Write-Host "🚀 Create Codespace: https://codespaces.new/phuongt/qagent-ai-test-system" -ForegroundColor Cyan
Write-Host ""
Write-Host "📋 Next steps:" -ForegroundColor Yellow
Write-Host "1. Visit the repository URL above" -ForegroundColor White
Write-Host "2. Click 'Code' button -> 'Codespaces' tab" -ForegroundColor White
Write-Host "3. Click 'Create codespace on main'" -ForegroundColor White
Write-Host "4. Wait for automatic setup (devcontainer will handle everything)" -ForegroundColor White
Write-Host "5. Run: cd qagent-app/QAgentWeb && dotnet run --urls 'http://0.0.0.0:5174'" -ForegroundColor White

Write-Host ""
Write-Host "🎉 Deploy completed successfully!" -ForegroundColor Green 