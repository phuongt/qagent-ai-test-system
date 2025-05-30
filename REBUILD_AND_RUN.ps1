# QAgent Rebuild & Run Script
# Muc dich: Rebuild va chay ung dung QAgentWeb tren port 5174
# Tac gia: AI Assistant
# Ngay tao: 28/05/2025

Write-Host "🚀 === QAGENT REBUILD & RUN SCRIPT ===" -ForegroundColor Cyan
Write-Host "📁 Preparing to rebuild and run QAgentWeb application..." -ForegroundColor Yellow
Write-Host ""

try {
    # Buoc 1: Dung tat ca process dotnet dang chay
    Write-Host "⏹️  Step 1: Stopping existing dotnet processes..." -ForegroundColor Yellow
    Get-Process -Name "*dotnet*" -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Host "✅ All dotnet processes stopped." -ForegroundColor Green
    Write-Host ""

    # Buoc 2: Chuyen den thu muc QAgentWeb
    Write-Host "📂 Step 2: Navigating to QAgentWeb directory..." -ForegroundColor Yellow
    $targetPath = "C:\Customize\01.QAgent\qagent-app\QAgentWeb"
    
    if (Test-Path $targetPath) {
        Set-Location $targetPath
        Write-Host "✅ Successfully changed to: $targetPath" -ForegroundColor Green
    } else {
        throw "❌ Directory not found: $targetPath"
    }
    Write-Host ""

    # Buoc 3: Clean project
    Write-Host "🧹 Step 3: Cleaning project..." -ForegroundColor Yellow
    dotnet clean
    if ($LASTEXITCODE -ne 0) {
        throw "❌ Clean failed with exit code: $LASTEXITCODE"
    }
    Write-Host "✅ Project cleaned successfully." -ForegroundColor Green
    Write-Host ""

    # Buoc 4: Restore packages
    Write-Host "📦 Step 4: Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "❌ Restore failed with exit code: $LASTEXITCODE"
    }
    Write-Host "✅ Packages restored successfully." -ForegroundColor Green
    Write-Host ""

    # Buoc 5: Build project
    Write-Host "🔨 Step 5: Building project..." -ForegroundColor Yellow
    dotnet build --configuration Release
    if ($LASTEXITCODE -ne 0) {
        throw "❌ Build failed with exit code: $LASTEXITCODE"
    }
    Write-Host "✅ Project built successfully." -ForegroundColor Green
    Write-Host ""

    # Buoc 6: Hien thi thong tin truoc khi chay
    Write-Host "🌐 Step 6: Starting application..." -ForegroundColor Yellow
    Write-Host "📋 Application Details:" -ForegroundColor Cyan
    Write-Host "   • URL: http://localhost:5174" -ForegroundColor White
    Write-Host "   • UC02 Page: http://localhost:5174/UC02" -ForegroundColor White
    Write-Host "   • Environment: Development" -ForegroundColor White
    Write-Host "   • Configuration: Release" -ForegroundColor White
    Write-Host ""
    Write-Host "⚠️  Press Ctrl+C to stop the application" -ForegroundColor Yellow
    Write-Host "🔄 Starting in 3 seconds..." -ForegroundColor Yellow
    Start-Sleep -Seconds 3
    Write-Host ""

    # Buoc 7: Chay ung dung
    Write-Host "🚀 Starting QAgentWeb application..." -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    dotnet run --urls "http://localhost:5174" --configuration Release

} catch {
    Write-Host ""
    Write-Host "❌ ERROR OCCURRED!" -ForegroundColor Red
    Write-Host "💥 Error Message: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🛠️  Troubleshooting Tips:" -ForegroundColor Yellow
    Write-Host "   1. Ensure you are running PowerShell as Administrator" -ForegroundColor White
    Write-Host "   2. Check if .NET 8 SDK is installed: dotnet --version" -ForegroundColor White
    Write-Host "   3. Verify the project path exists: $targetPath" -ForegroundColor White
    Write-Host "   4. Check for port conflicts: netstat -an | Select-String :5174" -ForegroundColor White
    Write-Host "   5. Try running individual commands manually" -ForegroundColor White
    Write-Host ""
    Write-Host "📞 For support, check the logs above for specific error details." -ForegroundColor Cyan
    
    # Pause de user co the doc error message
    Write-Host ""
    Write-Host "Press any key to exit..." -ForegroundColor Gray
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
} finally {
    Write-Host ""
    Write-Host "🏁 Script execution completed." -ForegroundColor Cyan
    Write-Host "📁 Current directory: $(Get-Location)" -ForegroundColor Gray
} 