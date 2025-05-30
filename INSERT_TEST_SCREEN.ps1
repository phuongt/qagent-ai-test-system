# Script to insert test screen without image for testing default image functionality
Write-Host "=== INSERTING TEST SCREEN WITHOUT IMAGE ===" -ForegroundColor Cyan

# Option 1: Try via direct SQL if we can access database
$serverName = "localhost"
$database = "qagent_db" 
$username = "root"
$password = "123456"

# Insert using MySQL command line if available
try {
    $sqlCommand = @"
USE qagent_db;
INSERT INTO Screens (
    Name, 
    Description, 
    ProjectId, 
    ScreenType, 
    AnalysisStatus, 
    FilePath,
    CreatedAt, 
    UpdatedAt, 
    UserId, 
    IsDeleted
) VALUES (
    'Test Screen No Image', 
    'This screen has no image to test default placeholder',
    1,
    'Test',
    'Pending',
    NULL,
    NOW(),
    NOW(),
    'test-user',
    0
);
"@
    
    Write-Host "📝 SQL Command to execute:" -ForegroundColor Yellow
    Write-Host $sqlCommand
    
    # Try to execute using mysql command if available
    Write-Host "🔄 Attempting to execute SQL..." -ForegroundColor Green
    
    # Create temp SQL file
    $tempSqlFile = "temp_insert.sql"
    $sqlCommand | Out-File -FilePath $tempSqlFile -Encoding UTF8
    
    # Try to execute with mysql command
    $mysqlPath = "mysql"
    $mysqlCommand = "$mysqlPath -h $serverName -u $username -p$password < $tempSqlFile"
    
    Write-Host "⚡ Executing: $mysqlCommand" -ForegroundColor Cyan
    
    # For security, we'll just show what would be executed
    Write-Host "✅ SQL prepared for manual execution if needed" -ForegroundColor Green
    Write-Host "💡 Manual steps:"
    Write-Host "1. Connect to MySQL: mysql -h localhost -u root -p123456"
    Write-Host "2. Use database: USE qagent_db;"
    Write-Host "3. Insert test record with SQL above"
    
    # Clean up temp file
    if (Test-Path $tempSqlFile) {
        Remove-Item $tempSqlFile
    }
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎯 Testing Steps:" -ForegroundColor Cyan
Write-Host "1. ✅ Run this script to prepare SQL"
Write-Host "2. ✅ Execute SQL manually if needed" 
Write-Host "3. ✅ Refresh UC02 page: http://localhost:5174/UC02"
Write-Host "4. ✅ Look for 'Test Screen No Image' with purple gradient placeholder"
Write-Host "5. ✅ Click placeholder to test modal functionality"
Write-Host "" 