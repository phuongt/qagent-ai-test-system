# Script to create test screen without image
Write-Host "=== CREATING TEST SCREEN WITHOUT IMAGE ===" -ForegroundColor Cyan

# Load necessary .NET assemblies
Add-Type -AssemblyName System.Data

try {
    # Create connection string
    $connectionString = "server=localhost;userid=root;password=123456;database=qagent_db"
    
    # Create MySql connection using .NET MySql connector
    $connection = New-Object MySql.Data.MySqlClient.MySqlConnection($connectionString)
    
    Write-Host "⚡ Connecting to database..." -ForegroundColor Yellow
    $connection.Open()
    
    # First, let's update an existing screen to have no FilePath
    $updateSql = "UPDATE Screens SET FilePath = NULL WHERE Name = 'Homepage Design'"
    $updateCommand = New-Object MySql.Data.MySqlClient.MySqlCommand($updateSql, $connection)
    $updateResult = $updateCommand.ExecuteNonQuery()
    
    Write-Host "✅ Updated Homepage Design screen to have no FilePath" -ForegroundColor Green
    Write-Host "📝 Rows affected: $updateResult" -ForegroundColor Green
    
    # Check the result
    $selectSql = "SELECT Name, FilePath FROM Screens WHERE Name = 'Homepage Design'"
    $selectCommand = New-Object MySql.Data.MySqlClient.MySqlCommand($selectSql, $connection)
    $reader = $selectCommand.ExecuteReader()
    
    Write-Host "📋 Verification:" -ForegroundColor Cyan
    while ($reader.Read()) {
        $name = $reader["Name"]
        $filePath = if ($reader["FilePath"] -eq [DBNull]::Value) { "NULL" } else { $reader["FilePath"] }
        Write-Host "   Name: $name, FilePath: $filePath" -ForegroundColor White
    }
    $reader.Close()
    
    $connection.Close()
    
    Write-Host ""
    Write-Host "🎉 SUCCESS! Homepage Design now has no image (FilePath = NULL)" -ForegroundColor Green
    Write-Host "🌐 Go to UC02 page to see the purple gradient default placeholder!" -ForegroundColor Cyan
    Write-Host "URL: http://localhost:5174/UC02" -ForegroundColor Yellow
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🛠️ Alternative method:" -ForegroundColor Yellow
    Write-Host "1. Use MySQL Workbench or phpMyAdmin" 
    Write-Host "2. Execute: UPDATE Screens SET FilePath = NULL WHERE Name = 'Homepage Design'"
    Write-Host "3. Refresh UC02 page to see default image"
} 