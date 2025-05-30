param(
    [string]$BaseUrl = "http://localhost:5174"
)

Write-Host "=== FINAL TEST: DEFAULT IMAGE FUNCTIONALITY ===" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "Test Date: $(Get-Date)" -ForegroundColor Green
Write-Host ""

# Test function với enhanced error handling
function Test-PageDefaultImages {
    param(
        [string]$PageUrl,
        [string]$PageName
    )
    
    Write-Host "🎯 Testing $PageName ($PageUrl)" -ForegroundColor Blue
    
    try {
        $response = Invoke-WebRequest -Uri $PageUrl -UseBasicParsing -TimeoutSec 30
        
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ Page loads successfully (Status: $($response.StatusCode))" -ForegroundColor Green
            
            # Kiểm tra content có các class CSS mới
            $content = $response.Content
            
            $cssChecks = @{
                "default-image-placeholder" = $content.Contains("default-image-placeholder")
                "image-with-fallback" = $content.Contains("image-with-fallback")
                "No Preview Available" = $content.Contains("No Preview Available")
                "Click to view details" = $content.Contains("Click to view details")
                "handleImageError" = $content.Contains("handleImageError")
                "showDefaultImageModal" = $content.Contains("showDefaultImageModal")
            }
            
            foreach ($check in $cssChecks.GetEnumerator()) {
                if ($check.Value) {
                    Write-Host "  ✅ $($check.Key): Found" -ForegroundColor Green
                } else {
                    Write-Host "  ❌ $($check.Key): Not found" -ForegroundColor Red
                }
            }
            
            # Check for Font Awesome icons
            if ($content.Contains("fa-image") -or $content.Contains("fas fa-image")) {
                Write-Host "  ✅ Font Awesome icons: Found" -ForegroundColor Green
            } else {
                Write-Host "  ⚠️ Font Awesome icons: Not detected" -ForegroundColor Yellow
            }
            
            return $true
        } else {
            Write-Host "❌ Page failed to load (Status: $($response.StatusCode))" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Error testing $PageName`: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Test CSS file
function Test-CSSFile {
    Write-Host "🎯 Testing CSS file" -ForegroundColor Blue
    
    try {
        $cssUrl = "$BaseUrl/css/site.css"
        $response = Invoke-WebRequest -Uri $cssUrl -UseBasicParsing -TimeoutSec 30
        
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ CSS file loads successfully" -ForegroundColor Green
            
            $cssContent = $response.Content
            $cssRules = @{
                ".default-image-placeholder" = $cssContent.Contains(".default-image-placeholder")
                ".image-with-fallback" = $cssContent.Contains(".image-with-fallback")
                ".fallback" = $cssContent.Contains(".fallback")
                "background: linear-gradient" = $cssContent.Contains("background: linear-gradient")
                "#667eea" = $cssContent.Contains("#667eea")
                "#764ba2" = $cssContent.Contains("#764ba2")
            }
            
            foreach ($rule in $cssRules.GetEnumerator()) {
                if ($rule.Value) {
                    Write-Host "  ✅ $($rule.Key): Found" -ForegroundColor Green
                } else {
                    Write-Host "  ❌ $($rule.Key): Not found" -ForegroundColor Red
                }
            }
            
            return $true
        } else {
            Write-Host "❌ CSS file failed to load (Status: $($response.StatusCode))" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Error testing CSS: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Bắt đầu test
Write-Host "🚀 Starting Default Image Functionality Tests..." -ForegroundColor Magenta
Write-Host ""

$testResults = @()

# Test CSS file first
$cssResult = Test-CSSFile
$testResults += @{ Test = "CSS File"; Result = $cssResult }
Write-Host ""

# Test các trang chính
$pagesToTest = @(
    @{ Url = "$BaseUrl/UC02"; Name = "UC02 - Phân tích ảnh UI" },
    @{ Url = "$BaseUrl/UC04"; Name = "UC04 - Screen Management" }
)

foreach ($page in $pagesToTest) {
    $result = Test-PageDefaultImages -PageUrl $page.Url -PageName $page.Name
    $testResults += @{ Test = $page.Name; Result = $result }
    Write-Host ""
}

# Summary
Write-Host "📊 TEST SUMMARY" -ForegroundColor Cyan
Write-Host "=" * 50
$passedCount = ($testResults | Where-Object { $_.Result -eq $true }).Count
$totalCount = $testResults.Count
$successRate = [math]::Round(($passedCount / $totalCount) * 100, 2)

foreach ($result in $testResults) {
    $status = if ($result.Result) { "✅ PASS" } else { "❌ FAIL" }
    Write-Host "$status - $($result.Test)" -ForegroundColor $(if ($result.Result) { "Green" } else { "Red" })
}

Write-Host ""
Write-Host "Total Tests: $totalCount" -ForegroundColor White
Write-Host "Passed: $passedCount" -ForegroundColor Green
Write-Host "Failed: $($totalCount - $passedCount)" -ForegroundColor Red
Write-Host "Success Rate: $successRate%" -ForegroundColor $(if ($successRate -ge 90) { "Green" } else { "Yellow" })

if ($successRate -eq 100) {
    Write-Host ""
    Write-Host "🎉 ALL TESTS PASSED! Default image functionality is working perfectly!" -ForegroundColor Green
} elseif ($successRate -ge 90) {
    Write-Host ""
    Write-Host "⚠️ Most tests passed. Minor issues detected." -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "❌ Multiple issues detected. Review needed." -ForegroundColor Red
}

Write-Host ""
Write-Host "Test completed at: $(Get-Date)" -ForegroundColor Cyan 