#!/usr/bin/env pwsh
# Oracle Database Test Runner Script
# This script runs comprehensive Oracle database tests for the QAgent application

param(
    [switch]$Verbose = $false,
    [switch]$Cleanup = $true,
    [switch]$NoCleanup = $false,
    [int]$Timeout = 30,
    [string]$ConnectionString = "",
    [string]$OracleHost = "localhost",
    [string]$OraclePort = "1521",
    [string]$OracleServiceName = "XEPDB1",
    [string]$OracleUser = "QAGENT_USER",
    [string]$OraclePassword = "TestPassword123",
    [switch]$Help = $false
)

# Color functions
function Write-Success { param($msg) Write-Host $msg -ForegroundColor Green }
function Write-Info { param($msg) Write-Host $msg -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host $msg -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host $msg -ForegroundColor Red }

function Show-Help {
    Write-Host "Oracle Database Test Runner" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Green
    Write-Host "  ./ORACLE_DB_TEST.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "OPTIONS:" -ForegroundColor Green
    Write-Host "  -Verbose                Enable verbose logging"
    Write-Host "  -Cleanup               Enable automatic cleanup after tests (default: true)"
    Write-Host "  -NoCleanup            Disable automatic cleanup"
    Write-Host "  -Timeout <minutes>    Set test timeout in minutes (default: 30)"
    Write-Host "  -ConnectionString <conn> Override connection string"
    Write-Host "  -OracleHost <host>    Oracle database host (default: localhost)"
    Write-Host "  -OraclePort <port>    Oracle database port (default: 1521)"
    Write-Host "  -OracleServiceName <service> Oracle service name (default: XEPDB1)"
    Write-Host "  -OracleUser <user>    Oracle username (default: QAGENT_USER)"
    Write-Host "  -OraclePassword <pass> Oracle password (default: TestPassword123)"
    Write-Host "  -Help                 Show this help message"
    Write-Host ""
    Write-Host "EXAMPLES:" -ForegroundColor Green
    Write-Host "  ./ORACLE_DB_TEST.ps1 -Verbose -Cleanup"
    Write-Host "  ./ORACLE_DB_TEST.ps1 -Timeout 60 -NoCleanup"
    Write-Host "  ./ORACLE_DB_TEST.ps1 -OracleHost 'oracle.example.com' -OraclePort 1521"
    Write-Host ""
    Write-Host "ENVIRONMENT VARIABLES:" -ForegroundColor Green
    Write-Host "  ORACLE_HOST           Oracle database host"
    Write-Host "  ORACLE_PORT           Oracle database port"
    Write-Host "  ORACLE_SERVICE_NAME   Oracle service name"
    Write-Host "  ORACLE_USER           Oracle username"
    Write-Host "  ORACLE_PASSWORD       Oracle password"
    Write-Host ""
}

function Test-Prerequisites {
    Write-Info "Checking prerequisites..."
    
    # Check if .NET 8 is installed
    try {
        $dotnetVersion = dotnet --version
        if ($dotnetVersion -notlike "8.*") {
            Write-Warning "Detected .NET version: $dotnetVersion"
            Write-Warning "This application requires .NET 8.0 or higher"
        } else {
            Write-Success "✓ .NET 8 is installed: $dotnetVersion"
        }
    } catch {
        Write-Error "✗ .NET CLI is not installed or not in PATH"
        return $false
    }
    
    # Check if project file exists
    $projectPath = "./qagent-app/QAgentWeb/QAgentWeb.csproj"
    if (-not (Test-Path $projectPath)) {
        Write-Error "✗ Project file not found: $projectPath"
        return $false
    }
    Write-Success "✓ Project file found: $projectPath"
    
    # Check if test files exist
    $testFiles = @(
        "./qagent-app/QAgentWeb/Tests/OracleDbTestConfig.cs",
        "./qagent-app/QAgentWeb/Tests/OracleDbTest.cs",
        "./qagent-app/QAgentWeb/Tests/OracleTestRunner.cs"
    )
    
    foreach ($file in $testFiles) {
        if (-not (Test-Path $file)) {
            Write-Error "✗ Test file not found: $file"
            return $false
        }
    }
    Write-Success "✓ All test files found"
    
    return $true
}

function Set-EnvironmentVariables {
    # Set Oracle environment variables
    $env:ORACLE_HOST = if ($OracleHost) { $OracleHost } else { $env:ORACLE_HOST -or "localhost" }
    $env:ORACLE_PORT = if ($OraclePort) { $OraclePort } else { $env:ORACLE_PORT -or "1521" }
    $env:ORACLE_SERVICE_NAME = if ($OracleServiceName) { $OracleServiceName } else { $env:ORACLE_SERVICE_NAME -or "XEPDB1" }
    $env:ORACLE_USER = if ($OracleUser) { $OracleUser } else { $env:ORACLE_USER -or "QAGENT_USER" }
    $env:ORACLE_PASSWORD = if ($OraclePassword) { $OraclePassword } else { $env:ORACLE_PASSWORD -or "TestPassword123" }
    
    Write-Info "Oracle Configuration:"
    Write-Info "  Host: $env:ORACLE_HOST"
    Write-Info "  Port: $env:ORACLE_PORT"
    Write-Info "  Service: $env:ORACLE_SERVICE_NAME"
    Write-Info "  User: $env:ORACLE_USER"
    Write-Info "  Password: $($env:ORACLE_PASSWORD.Substring(0, 3))***"
}

function Build-Project {
    Write-Info "Building project..."
    
    Push-Location "./qagent-app/QAgentWeb"
    
    try {
        # Restore packages
        Write-Info "Restoring NuGet packages..."
        dotnet restore
        if ($LASTEXITCODE -ne 0) {
            Write-Error "✗ Package restore failed"
            return $false
        }
        
        # Build project
        Write-Info "Building project..."
        dotnet build --no-restore
        if ($LASTEXITCODE -ne 0) {
            Write-Error "✗ Build failed"
            return $false
        }
        
        Write-Success "✓ Project built successfully"
        return $true
    } catch {
        Write-Error "✗ Build process failed: $($_.Exception.Message)"
        return $false
    } finally {
        Pop-Location
    }
}

function Run-OracleTests {
    Write-Info "Running Oracle database tests..."
    
    Push-Location "./qagent-app/QAgentWeb"
    
    try {
        # Build test arguments
        $testArgs = @()
        
        if ($Verbose) { $testArgs += "--verbose" }
        if ($NoCleanup) { $testArgs += "--no-cleanup" }
        elseif ($Cleanup) { $testArgs += "--cleanup" }
        if ($Timeout -gt 0) { $testArgs += "--timeout", $Timeout }
        if ($ConnectionString) { $testArgs += "--connection", $ConnectionString }
        
        # Run tests
        Write-Info "Executing tests with arguments: $($testArgs -join ' ')"
        
        # Create a simple test runner since we don't have a separate test project
        $testCode = @"
using QAgentWeb.Tests;

var test = new OracleDbTest();
var result = await test.RunCompleteTest();
Environment.Exit(result ? 0 : 1);
"@
        
        # Save test code to temp file
        $tempTestFile = [System.IO.Path]::GetTempFileName() + ".cs"
        $testCode | Out-File -FilePath $tempTestFile -Encoding UTF8
        
        # Run using dotnet script or similar approach
        # For now, let's use a simpler approach - compile and run
        $testRunnerPath = "./Tests/OracleTestRunner.cs"
        
        # Check if we can compile the test runner
        if (Test-Path $testRunnerPath) {
            Write-Info "Compiling and running test runner..."
            
            # Use dotnet run with the test runner
            $startInfo = New-Object System.Diagnostics.ProcessStartInfo
            $startInfo.FileName = "dotnet"
            $startInfo.Arguments = "run --project . -- $($testArgs -join ' ')"
            $startInfo.RedirectStandardOutput = $true
            $startInfo.RedirectStandardError = $true
            $startInfo.UseShellExecute = $false
            $startInfo.CreateNoWindow = $true
            
            $process = New-Object System.Diagnostics.Process
            $process.StartInfo = $startInfo
            
            $outputHandler = {
                param($sender, $e)
                if (-not [string]::IsNullOrEmpty($e.Data)) {
                    Write-Host $e.Data
                }
            }
            
            $errorHandler = {
                param($sender, $e)
                if (-not [string]::IsNullOrEmpty($e.Data)) {
                    Write-Host $e.Data -ForegroundColor Red
                }
            }
            
            $process.add_OutputDataReceived($outputHandler)
            $process.add_ErrorDataReceived($errorHandler)
            
            $process.Start()
            $process.BeginOutputReadLine()
            $process.BeginErrorReadLine()
            
            # Wait for completion with timeout
            $timeoutMs = $Timeout * 60 * 1000
            if (-not $process.WaitForExit($timeoutMs)) {
                Write-Error "Test execution timed out after $Timeout minutes"
                $process.Kill()
                return $false
            }
            
            $exitCode = $process.ExitCode
            
            if ($exitCode -eq 0) {
                Write-Success "✓ All tests passed"
                return $true
            } else {
                Write-Error "✗ Tests failed with exit code: $exitCode"
                return $false
            }
        } else {
            Write-Error "✗ Test runner not found: $testRunnerPath"
            return $false
        }
    } catch {
        Write-Error "✗ Test execution failed: $($_.Exception.Message)"
        return $false
    } finally {
        Pop-Location
        # Clean up temp files
        if (Test-Path $tempTestFile) {
            Remove-Item $tempTestFile -Force
        }
    }
}

function Show-TestResults {
    param([bool]$Success)
    
    Write-Host ""
    Write-Host "=" * 60
    Write-Host "TEST EXECUTION SUMMARY" -ForegroundColor Yellow
    Write-Host "=" * 60
    
    if ($Success) {
        Write-Success "✓ ORACLE DATABASE TESTS PASSED"
        Write-Info "All expected record counts match."
        Write-Info "Database connections and operations are working correctly."
        Write-Info "Test data has been created and verified successfully."
    } else {
        Write-Error "✗ ORACLE DATABASE TESTS FAILED"
        Write-Warning "Please check the test output for detailed error information."
        Write-Warning "Common issues:"
        Write-Warning "  - Oracle database is not running"
        Write-Warning "  - Connection string is incorrect"
        Write-Warning "  - Database user does not have sufficient privileges"
        Write-Warning "  - Required tables or schema are missing"
    }
    
    Write-Host ""
    Write-Info "Test report files are generated in the project directory."
    Write-Info "Check Oracle_Test_Report_*.md for detailed results."
    Write-Host ""
}

# Main execution
function Main {
    if ($Help) {
        Show-Help
        return
    }
    
    # Handle NoCleanup flag
    if ($NoCleanup) {
        $Cleanup = $false
    }
    
    Write-Host "=" * 60 -ForegroundColor Yellow
    Write-Host "ORACLE DATABASE TEST RUNNER" -ForegroundColor Yellow
    Write-Host "=" * 60 -ForegroundColor Yellow
    Write-Host "Started at: $(Get-Date)" -ForegroundColor Cyan
    Write-Host ""
    
    # Check prerequisites
    if (-not (Test-Prerequisites)) {
        Write-Error "Prerequisites check failed. Cannot continue."
        exit 1
    }
    
    # Set environment variables
    Set-EnvironmentVariables
    
    # Build project
    if (-not (Build-Project)) {
        Write-Error "Build failed. Cannot continue."
        exit 1
    }
    
    # Run tests
    $testResult = Run-OracleTests
    
    # Show results
    Show-TestResults -Success $testResult
    
    # Exit with appropriate code
    if ($testResult) {
        Write-Success "Oracle database tests completed successfully!"
        exit 0
    } else {
        Write-Error "Oracle database tests failed!"
        exit 1
    }
}

# Execute main function
Main