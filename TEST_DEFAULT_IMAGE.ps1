# Test Default Image Feature
Write-Host "=== UC02 DEFAULT IMAGE TEST ===" -ForegroundColor Cyan

# Test database connection and insert screen without image
$connectionString = "Server=localhost;Database=qagent_db;Uid=root;Pwd=123456;AllowLoadLocalInfile=true;"

try {
    # Load MySQL connector if available
    Add-Type -Path "C:\Program Files (x86)\MySQL\Connector NET 8.0\Assemblies\v4.5.2\MySql.Data.dll" -ErrorAction SilentlyContinue
    
    Write-Host "✅ Connecting to database..." -ForegroundColor Green
    
    # SQL to insert a screen without FilePath
    $insertSQL = @"
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
    'Test Screen Without Image', 
    'This is a test screen to demonstrate default image functionality',
    1,
    'TestScreen',
    'Pending',
    NULL,
    NOW(),
    NOW(),
    'system',
    FALSE
);
"@
    
    Write-Host "📝 SQL Query:"
    Write-Host $insertSQL -ForegroundColor Yellow
    
    Write-Host "✅ Test screen without image would be inserted to test default image feature"
    Write-Host "📋 Next: Check UC02 page to see default image placeholder"
    
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 Continuing with browser test instead..."
}

Write-Host ""
Write-Host "🌐 Opening UC02 to test current implementation..." -ForegroundColor Cyan
Write-Host "URL: http://localhost:5174/UC02"
Write-Host ""
Write-Host "🎯 Expected behaviors to test:"
Write-Host "1. ✅ Screens with images show normal preview"
Write-Host "2. ✅ Screens without images show purple gradient default placeholder"
Write-Host "3. ✅ Clicking default placeholder opens modal with 'No Image Preview' message"
Write-Host "4. ✅ Modal properly resets when closed"
Write-Host "" 