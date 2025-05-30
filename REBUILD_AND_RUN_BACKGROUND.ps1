# QAgent Rebuild & Run Background Script
# Muc dich: Rebuild va chay ung dung QAgentWeb tren port 5174 o che do background
# Tac gia: AI Assistant
# Ngay tao: 28/05/2025

Write-Host "🚀 === QAGENT REBUILD & RUN BACKGROUND SCRIPT ===" -ForegroundColor Cyan
Write-Host "📁 Preparing to rebuild and run QAgentWeb application in background..." -ForegroundColor Yellow
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
    dotnet clean | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "❌ Clean failed with exit code: $LASTEXITCODE"
    }
    Write-Host "✅ Project cleaned successfully." -ForegroundColor Green
    Write-Host ""

    # Buoc 4: Restore packages
    Write-Host "📦 Step 4: Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "❌ Restore failed with exit code: $LASTEXITCODE"
    }
    Write-Host "✅ Packages restored successfully." -ForegroundColor Green
    Write-Host ""

    # Buoc 5: Build project
    Write-Host "🔨 Step 5: Building project..." -ForegroundColor Yellow
    dotnet build --configuration Release | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "❌ Build failed with exit code: $LASTEXITCODE"
    }
    Write-Host "✅ Project built successfully." -ForegroundColor Green
    Write-Host ""

    # Buoc 6: Chay ung dung o che do background
    Write-Host "🌐 Step 6: Starting application in BACKGROUND mode..." -ForegroundColor Yellow
    Write-Host "📋 Application Details:" -ForegroundColor Cyan
    Write-Host "   • URL: http://localhost:5174" -ForegroundColor White
    Write-Host "   • UC02 Page: http://localhost:5174/UC02" -ForegroundColor White
    Write-Host "   • Environment: Development" -ForegroundColor White
    Write-Host "   • Configuration: Release" -ForegroundColor White
    Write-Host "   • Mode: BACKGROUND JOB" -ForegroundColor Magenta
    Write-Host ""

    # Tao background job de chay ung dung
    $job = Start-Job -ScriptBlock {
        Set-Location "C:\Customize\01.QAgent\qagent-app\QAgentWeb"
        dotnet run --urls "http://localhost:5174" --configuration Release
    }

    Write-Host "🚀 QAgentWeb started as background job!" -ForegroundColor Green
    Write-Host "📊 Job ID: $($job.Id)" -ForegroundColor Cyan
    Write-Host "📊 Job Name: $($job.Name)" -ForegroundColor Cyan
    Write-Host ""

    # Doi 10 giay de ung dung khoi dong
    Write-Host "⏳ Waiting 10 seconds for application to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10

    # Kiem tra trang thai job
    $jobStatus = Get-Job -Id $job.Id
    Write-Host "📊 Job Status: $($jobStatus.State)" -ForegroundColor Cyan

    if ($jobStatus.State -eq "Running") {
        Write-Host "✅ Application is running successfully in background!" -ForegroundColor Green
        Write-Host ""
        Write-Host "🔧 Management Commands:" -ForegroundColor Yellow
        Write-Host "   • Check status: Get-Job -Id $($job.Id)" -ForegroundColor White
        Write-Host "   • View output: Receive-Job -Id $($job.Id) -Keep" -ForegroundColor White
        Write-Host "   • Stop app: Stop-Job -Id $($job.Id); Remove-Job -Id $($job.Id)" -ForegroundColor White
        Write-Host "   • Test URL: curl http://localhost:5174" -ForegroundColor White
        Write-Host ""
        Write-Host "🌐 Application URLs:" -ForegroundColor Green
        Write-Host "   • Main: http://localhost:5174" -ForegroundColor White
        Write-Host "   • UC02: http://localhost:5174/UC02" -ForegroundColor White
    } else {
        Write-Host "❌ Application failed to start properly." -ForegroundColor Red
        Write-Host "📋 Job Output:" -ForegroundColor Yellow
        Receive-Job -Id $job.Id
    }

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
} finally {
    Write-Host ""
    Write-Host "🏁 Script execution completed." -ForegroundColor Cyan
    Write-Host "📁 Current directory: $(Get-Location)" -ForegroundColor Gray
} 