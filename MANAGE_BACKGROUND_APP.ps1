# QAgent Background Job Management Script
# Muc dich: Quan ly background jobs cua ung dung QAgentWeb
# Tac gia: AI Assistant
# Ngay tao: 28/05/2025

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("status", "stop", "logs", "restart", "list")]
    [string]$Action
)

Write-Host "🎛️  === QAGENT BACKGROUND JOB MANAGER ===" -ForegroundColor Cyan
Write-Host "⚙️  Action: $Action" -ForegroundColor Yellow
Write-Host ""

try {
    switch ($Action) {
        "status" {
            Write-Host "📊 Checking application status..." -ForegroundColor Yellow
            
            # Kiem tra jobs
            $jobs = Get-Job | Where-Object { $_.Name -like "*Job*" }
            if ($jobs) {
                Write-Host "✅ Found $($jobs.Count) background job(s):" -ForegroundColor Green
                foreach ($job in $jobs) {
                    Write-Host "   • Job ID: $($job.Id) | State: $($job.State) | Name: $($job.Name)" -ForegroundColor White
                }
            } else {
                Write-Host "❌ No background jobs found." -ForegroundColor Red
            }
            
            # Kiem tra port 5174
            Write-Host ""
            Write-Host "🌐 Checking port 5174..." -ForegroundColor Yellow
            $portCheck = netstat -an | Select-String ":5174"
            if ($portCheck) {
                Write-Host "✅ Port 5174 is in use:" -ForegroundColor Green
                $portCheck | ForEach-Object { Write-Host "   $($_)" -ForegroundColor White }
            } else {
                Write-Host "❌ Port 5174 is not in use." -ForegroundColor Red
            }
        }
        
        "stop" {
            Write-Host "⏹️  Stopping all QAgent background jobs..." -ForegroundColor Yellow
            
            # Dung tat ca background jobs
            $jobs = Get-Job
            if ($jobs) {
                foreach ($job in $jobs) {
                    Write-Host "⏹️  Stopping Job ID: $($job.Id)" -ForegroundColor Yellow
                    Stop-Job -Id $job.Id -ErrorAction SilentlyContinue
                    Remove-Job -Id $job.Id -ErrorAction SilentlyContinue
                }
                Write-Host "✅ All background jobs stopped." -ForegroundColor Green
            } else {
                Write-Host "ℹ️  No background jobs to stop." -ForegroundColor Blue
            }
            
            # Dung tat ca dotnet processes
            Write-Host "⏹️  Stopping all dotnet processes..." -ForegroundColor Yellow
            Get-Process -Name "*dotnet*" -ErrorAction SilentlyContinue | Stop-Process -Force
            Write-Host "✅ All dotnet processes stopped." -ForegroundColor Green
        }
        
        "logs" {
            Write-Host "📋 Retrieving application logs..." -ForegroundColor Yellow
            
            $jobs = Get-Job
            if ($jobs) {
                foreach ($job in $jobs) {
                    Write-Host "📋 Logs for Job ID: $($job.Id)" -ForegroundColor Cyan
                    Write-Host "═══════════════════════════════════════" -ForegroundColor Gray
                    Receive-Job -Id $job.Id -Keep | Out-String | Write-Host
                    Write-Host "═══════════════════════════════════════" -ForegroundColor Gray
                }
            } else {
                Write-Host "❌ No background jobs found to retrieve logs from." -ForegroundColor Red
            }
        }
        
        "restart" {
            Write-Host "🔄 Restarting QAgent application..." -ForegroundColor Yellow
            
            # Dung tat ca jobs va processes
            Write-Host "⏹️  Stopping existing instances..." -ForegroundColor Yellow
            Get-Job | Stop-Job -ErrorAction SilentlyContinue
            Get-Job | Remove-Job -ErrorAction SilentlyContinue
            Get-Process -Name "*dotnet*" -ErrorAction SilentlyContinue | Stop-Process -Force
            Start-Sleep -Seconds 3
            
            # Chay lai background script
            Write-Host "🚀 Starting new background instance..." -ForegroundColor Yellow
            & ".\REBUILD_AND_RUN_BACKGROUND.ps1"
        }
        
        "list" {
            Write-Host "📋 Listing all PowerShell jobs..." -ForegroundColor Yellow
            
            $allJobs = Get-Job
            if ($allJobs) {
                Write-Host "✅ Found $($allJobs.Count) total job(s):" -ForegroundColor Green
                $allJobs | Format-Table Id, Name, State, HasMoreData, Location -AutoSize
            } else {
                Write-Host "❌ No PowerShell jobs found." -ForegroundColor Red
            }
            
            # Hien thi tat ca dotnet processes
            Write-Host ""
            Write-Host "🔧 Active dotnet processes:" -ForegroundColor Yellow
            $dotnetProcesses = Get-Process -Name "*dotnet*" -ErrorAction SilentlyContinue
            if ($dotnetProcesses) {
                $dotnetProcesses | Format-Table Id, ProcessName, CPU, WorkingSet -AutoSize
            } else {
                Write-Host "❌ No dotnet processes found." -ForegroundColor Red
            }
        }
    }
    
} catch {
    Write-Host ""
    Write-Host "❌ ERROR OCCURRED!" -ForegroundColor Red
    Write-Host "💥 Error Message: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Write-Host ""
    Write-Host "🏁 Management operation completed." -ForegroundColor Cyan
}

# Hien thi huong dan su dung
Write-Host ""
Write-Host "📖 Usage Examples:" -ForegroundColor Green
Write-Host "   .\MANAGE_BACKGROUND_APP.ps1 -Action status   # Check app status" -ForegroundColor White
Write-Host "   .\MANAGE_BACKGROUND_APP.ps1 -Action stop     # Stop all instances" -ForegroundColor White
Write-Host "   .\MANAGE_BACKGROUND_APP.ps1 -Action logs     # View application logs" -ForegroundColor White
Write-Host "   .\MANAGE_BACKGROUND_APP.ps1 -Action restart  # Restart application" -ForegroundColor White
Write-Host "   .\MANAGE_BACKGROUND_APP.ps1 -Action list     # List all jobs/processes" -ForegroundColor White 