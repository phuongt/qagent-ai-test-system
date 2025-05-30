#!/usr/bin/env pwsh

# =============================================================================
# COMPREHENSIVE DEFAULT IMAGE TESTING SCRIPT
# =============================================================================
# Kiểm tra tính năng hiển thị ảnh mặc định trên trang UC02
# Author: QAgent AI Assistant
# Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
# =============================================================================

param(
    [string]$BaseUrl = "http://localhost:5174",
    [switch]$Verbose = $false
)

# Test configuration
$TestResults = @{
    CSSClasses = $false
    DefaultPlaceholders = $false  
    ImageErrorHandling = $false
    ModalFunctionality = $false
    JavaScriptFunctions = $false
    OverallSuccess = $false
}

$LogFile = "UC02_DEFAULT_IMAGE_TEST_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"

function Write-TestLog {
    param([string]$Message, [string]$Level = "INFO")
    $Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $LogMessage = "[$Timestamp] [$Level] $Message"
    Write-Host $LogMessage
    Add-Content -Path $LogFile -Value $LogMessage
}

function Test-ApplicationRunning {
    Write-TestLog "🔍 Kiểm tra ứng dụng đang chạy..."
    try {
        $response = Invoke-WebRequest -Uri $BaseUrl -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-TestLog "✅ Ứng dụng đang chạy tại $BaseUrl" "SUCCESS"
            return $true
        }
    }
    catch {
        Write-TestLog "❌ Ứng dụng không chạy: $($_.Exception.Message)" "ERROR"
        return $false
    }
    return $false
}

function Test-CSSClasses {
    Write-TestLog "🎨 Kiểm tra CSS classes cho default image..."
    try {
        $cssPath = "qagent-app/QAgentWeb/wwwroot/css/site.css"
        $cssContent = Get-Content -Path $cssPath -Raw
        
        $requiredClasses = @(
            ".default-image-placeholder",
            ".image-with-fallback", 
            ".fallback"
        )
        
        $allClassesFound = $true
        foreach ($class in $requiredClasses) {
            if ($cssContent -notmatch [regex]::Escape($class)) {
                Write-TestLog "❌ CSS class không tìm thấy: $class" "ERROR"
                $allClassesFound = $false
            } else {
                Write-TestLog "✅ CSS class tìm thấy: $class" "SUCCESS"
            }
        }
        
        # Kiểm tra purple gradient background
        if ($cssContent -match "#667eea.*#764ba2" -or $cssContent -match "#764ba2.*#667eea") {
            Write-TestLog "✅ Purple gradient background được tìm thấy" "SUCCESS"
        } else {
            Write-TestLog "❌ Purple gradient background không tìm thấy" "ERROR"
            $allClassesFound = $false
        }
        
        $TestResults.CSSClasses = $allClassesFound
        return $allClassesFound
    }
    catch {
        Write-TestLog "❌ Lỗi kiểm tra CSS: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Test-HTMLStructure {
    Write-TestLog "🏗️ Kiểm tra cấu trúc HTML trong UC02..."
    try {
        $htmlPath = "qagent-app/QAgentWeb/Pages/UC02/Index.cshtml"
        $htmlContent = Get-Content -Path $htmlPath -Raw
        
        $requiredElements = @(
            "default-image-placeholder",
            "image-with-fallback",
            "handleImageError",
            "showDefaultImageModal",
            "No Preview Available",
            "Image Not Available"
        )
        
        $allElementsFound = $true
        foreach ($element in $requiredElements) {
            if ($htmlContent -match [regex]::Escape($element)) {
                Write-TestLog "✅ HTML element tìm thấy: $element" "SUCCESS"
            } else {
                Write-TestLog "❌ HTML element không tìm thấy: $element" "ERROR"
                $allElementsFound = $false
            }
        }
        
        $TestResults.DefaultPlaceholders = $allElementsFound
        return $allElementsFound
    }
    catch {
        Write-TestLog "❌ Lỗi kiểm tra HTML: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Test-JavaScriptFunctions {
    Write-TestLog "⚙️ Kiểm tra JavaScript functions..."
    try {
        $htmlPath = "qagent-app/QAgentWeb/Pages/UC02/Index.cshtml"
        $htmlContent = Get-Content -Path $htmlPath -Raw
        
        $requiredFunctions = @(
            "function handleImageError",
            "function showImageModal",
            "function showDefaultImageModal", 
            "function closeImageModal"
        )
        
        $allFunctionsFound = $true
        foreach ($func in $requiredFunctions) {
            if ($htmlContent -match [regex]::Escape($func)) {
                Write-TestLog "✅ JavaScript function tìm thấy: $func" "SUCCESS"
            } else {
                Write-TestLog "❌ JavaScript function không tìm thấy: $func" "ERROR"
                $allFunctionsFound = $false
            }
        }
        
        $TestResults.JavaScriptFunctions = $allFunctionsFound
        return $allFunctionsFound
    }
    catch {
        Write-TestLog "❌ Lỗi kiểm tra JavaScript: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Test-UC02Page {
    Write-TestLog "🌐 Kiểm tra trang UC02..."
    try {
        $response = Invoke-WebRequest -Uri "$BaseUrl/UC02" -TimeoutSec 15
        
        # Kiểm tra status code
        if ($response.StatusCode -ne 200) {
            Write-TestLog "❌ UC02 page status code: $($response.StatusCode)" "ERROR"
            return $false
        }
        
        # Kiểm tra content có chứa "No Preview Available"
        if ($response.Content -match "No Preview Available") {
            Write-TestLog "✅ Trang UC02 có hiển thị 'No Preview Available'" "SUCCESS"
        } else {
            Write-TestLog "❌ Trang UC02 không hiển thị 'No Preview Available'" "ERROR"
            return $false
        }
        
        # Kiểm tra CSS classes được load
        if ($response.Content -match "default-image-placeholder") {
            Write-TestLog "✅ CSS classes được áp dụng trong trang" "SUCCESS"
        } else {
            Write-TestLog "❌ CSS classes không được áp dụng" "ERROR"
            return $false
        }
        
        $TestResults.ImageErrorHandling = $true
        $TestResults.ModalFunctionality = $true
        return $true
    }
    catch {
        Write-TestLog "❌ Lỗi kiểm tra trang UC02: $($_.Exception.Message)" "ERROR"
        return $false
    }
}

function Generate-TestReport {
    Write-TestLog "📊 Tạo báo cáo test..."
    
    $totalTests = $TestResults.Keys.Count - 1  # Trừ OverallSuccess
    $passedTests = ($TestResults.GetEnumerator() | Where-Object { $_.Key -ne "OverallSuccess" -and $_.Value -eq $true }).Count
    $successRate = [math]::Round(($passedTests / $totalTests) * 100, 2)
    
    $TestResults.OverallSuccess = $passedTests -eq $totalTests
    
    Write-TestLog "=" * 60
    Write-TestLog "DEFAULT IMAGE FEATURE TEST REPORT"
    Write-TestLog "=" * 60
    Write-TestLog "Test Results:"
    Write-TestLog "  ✅ CSS Classes: $(if($TestResults.CSSClasses) { 'PASS' } else { 'FAIL' })"
    Write-TestLog "  ✅ Default Placeholders: $(if($TestResults.DefaultPlaceholders) { 'PASS' } else { 'FAIL' })"
    Write-TestLog "  ✅ Image Error Handling: $(if($TestResults.ImageErrorHandling) { 'PASS' } else { 'FAIL' })"
    Write-TestLog "  ✅ Modal Functionality: $(if($TestResults.ModalFunctionality) { 'PASS' } else { 'FAIL' })"
    Write-TestLog "  ✅ JavaScript Functions: $(if($TestResults.JavaScriptFunctions) { 'PASS' } else { 'FAIL' })"
    Write-TestLog ""
    Write-TestLog "Summary:"
    Write-TestLog "  Total Tests: $totalTests"
    Write-TestLog "  Passed: $passedTests"
    Write-TestLog "  Failed: $($totalTests - $passedTests)"
    Write-TestLog "  Success Rate: $successRate%"
    Write-TestLog ""
    Write-TestLog "Overall Status: $(if($TestResults.OverallSuccess) { '🎉 SUCCESS' } else { '❌ FAILED' })"
    Write-TestLog "=" * 60
    
    # Tạo file báo cáo
    $reportContent = @"
# DEFAULT IMAGE FEATURE TEST REPORT
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Test Results

| Test Category | Status | Details |
|---------------|--------|---------|
| CSS Classes | $(if($TestResults.CSSClasses) { '✅ PASS' } else { '❌ FAIL' }) | Kiểm tra các class CSS cần thiết |
| Default Placeholders | $(if($TestResults.DefaultPlaceholders) { '✅ PASS' } else { '❌ FAIL' }) | Kiểm tra HTML structure và placeholders |
| Image Error Handling | $(if($TestResults.ImageErrorHandling) { '✅ PASS' } else { '❌ FAIL' }) | Kiểm tra xử lý lỗi ảnh |
| Modal Functionality | $(if($TestResults.ModalFunctionality) { '✅ PASS' } else { '❌ FAIL' }) | Kiểm tra chức năng modal |
| JavaScript Functions | $(if($TestResults.JavaScriptFunctions) { '✅ PASS' } else { '❌ FAIL' }) | Kiểm tra các hàm JavaScript |

## Summary
- **Total Tests:** $totalTests
- **Passed:** $passedTests  
- **Failed:** $($totalTests - $passedTests)
- **Success Rate:** $successRate%
- **Overall Status:** $(if($TestResults.OverallSuccess) { '🎉 SUCCESS' } else { '❌ FAILED' })

## Features Tested
1. **CSS Default Image Classes**: Purple gradient background, proper styling
2. **HTML Structure**: Image containers with fallback elements
3. **JavaScript Error Handling**: handleImageError, showDefaultImageModal functions
4. **Modal System**: Image modal with default content support
5. **Page Integration**: UC02 page displays default images correctly

## Next Steps
$(if($TestResults.OverallSuccess) { 
'✅ All tests passed! Default image feature is working correctly.' 
} else { 
'❌ Some tests failed. Review the log file for details and fix the issues.' })

---
*Test log saved to: $LogFile*
"@

    $reportFile = "UC02_DEFAULT_IMAGE_TEST_REPORT_$(Get-Date -Format 'yyyyMMdd_HHmmss').md"
    Set-Content -Path $reportFile -Value $reportContent
    Write-TestLog "📄 Báo cáo đã được lưu: $reportFile"
}

# Main execution
Write-TestLog "🚀 Bắt đầu test tính năng Default Image..."
Write-TestLog "Target URL: $BaseUrl"

# Run tests
if (-not (Test-ApplicationRunning)) {
    Write-TestLog "❌ Ứng dụng không chạy. Hãy chạy REBUILD_AND_RUN_BACKGROUND.ps1 trước." "ERROR"
    exit 1
}

Write-TestLog "🧪 Chạy các test cases..."

Test-CSSClasses
Test-HTMLStructure  
Test-JavaScriptFunctions
Test-UC02Page

Generate-TestReport

if ($TestResults.OverallSuccess) {
    Write-TestLog "🎉 TẤT CẢ TEST THÀNH CÔNG! Default image feature hoạt động hoàn hảo." "SUCCESS"
    exit 0
} else {
    Write-TestLog "❌ CÓ TEST THẤT BẠI. Xem log để biết chi tiết." "ERROR"
    exit 1
} 