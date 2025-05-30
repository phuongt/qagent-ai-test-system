# Quick Run Script - Chạy nhanh QAgentWeb 
# Sử dụng khi đã build rồi và chỉ cần chạy lại

Write-Host "⚡ === QUICK RUN QAGENT ===" -ForegroundColor Green

# Dừng process cũ
Get-Process -Name "*dotnet*" -ErrorAction SilentlyContinue | Stop-Process -Force

# CD đến folder QAgentWeb
Set-Location "C:\Customize\01.QAgent\qagent-app\QAgentWeb"

# Hiển thị thông tin
Write-Host "🌐 Starting QAgentWeb on http://localhost:5174" -ForegroundColor Cyan
Write-Host "📋 UC02 Page: http://localhost:5174/UC02" -ForegroundColor Yellow
Write-Host ""

# Chạy ứng dụng
dotnet run --urls "http://localhost:5174" 