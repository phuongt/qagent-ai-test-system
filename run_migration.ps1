# =============================================
# QAgent Database Migration Script
# PowerShell script để chạy migration MySQL
# =============================================

Write-Host "=== QAgent Database Migration ===" -ForegroundColor Green
Write-Host "Connecting to MySQL database..." -ForegroundColor Yellow

# Database connection parameters
$Server = "sql12.freesqldatabase.com"
$Port = "3306"
$Database = "sql12781385"
$Username = "sql12781385"
$Password = "nQS9fRRZZ7"
$SqlFile = "database_migration_and_seed.sql"

# Check if MySQL client is available
$mysqlPath = Get-Command mysql -ErrorAction SilentlyContinue
if (-not $mysqlPath) {
    Write-Host "ERROR: MySQL client not found!" -ForegroundColor Red
    Write-Host "Please install MySQL client or add it to PATH" -ForegroundColor Yellow
    Write-Host "Download from: https://dev.mysql.com/downloads/mysql/" -ForegroundColor Cyan
    exit 1
}

# Check if SQL file exists
if (-not (Test-Path $SqlFile)) {
    Write-Host "ERROR: SQL file '$SqlFile' not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Found MySQL client: $($mysqlPath.Source)" -ForegroundColor Green
Write-Host "SQL file: $SqlFile" -ForegroundColor Green

# Confirm before running
Write-Host "`nDatabase Details:" -ForegroundColor Cyan
Write-Host "  Server: $Server" -ForegroundColor White
Write-Host "  Port: $Port" -ForegroundColor White
Write-Host "  Database: $Database" -ForegroundColor White
Write-Host "  Username: $Username" -ForegroundColor White

$confirm = Read-Host "`nDo you want to run the migration? (y/N)"
if ($confirm -ne 'y' -and $confirm -ne 'Y') {
    Write-Host "Migration cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host "`nRunning migration..." -ForegroundColor Yellow

try {
    # Run MySQL command
    $mysqlCommand = "mysql -h $Server -P $Port -u $Username -p$Password $Database"
    
    Write-Host "Executing: $mysqlCommand < $SqlFile" -ForegroundColor Gray
    
    # Execute the SQL file
    $result = cmd /c "$mysqlCommand < $SqlFile 2>&1"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n=== MIGRATION SUCCESSFUL ===" -ForegroundColor Green
        Write-Host "Database tables created and seeded successfully!" -ForegroundColor Green
        
        # Show result if any
        if ($result) {
            Write-Host "`nOutput:" -ForegroundColor Cyan
            Write-Host $result -ForegroundColor White
        }
        
        Write-Host "`nNext steps:" -ForegroundColor Yellow
        Write-Host "1. Update your appsettings.json with correct connection string" -ForegroundColor White
        Write-Host "2. Build and run your .NET application" -ForegroundColor White
        Write-Host "3. Test the database connection" -ForegroundColor White
        
    } else {
        Write-Host "`n=== MIGRATION FAILED ===" -ForegroundColor Red
        Write-Host "Error output:" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "`n=== MIGRATION FAILED ===" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`nMigration completed!" -ForegroundColor Green 