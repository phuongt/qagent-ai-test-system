@echo off
REM =============================================
REM QAgent Database Migration Script (Batch)
REM Alternative script for Windows
REM =============================================

echo === QAgent Database Migration ===
echo Connecting to MySQL database...

REM Database connection parameters
set SERVER=sql12.freesqldatabase.com
set PORT=3306
set DATABASE=sql12781385
set USERNAME=sql12781385
set PASSWORD=nQS9fRRZZ7
set SQLFILE=database_migration_and_seed.sql

REM Check if MySQL client is available
mysql --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: MySQL client not found!
    echo Please install MySQL client or add it to PATH
    echo Download from: https://dev.mysql.com/downloads/mysql/
    pause
    exit /b 1
)

REM Check if SQL file exists
if not exist "%SQLFILE%" (
    echo ERROR: SQL file '%SQLFILE%' not found!
    pause
    exit /b 1
)

echo Found MySQL client
echo SQL file: %SQLFILE%

echo.
echo Database Details:
echo   Server: %SERVER%
echo   Port: %PORT%
echo   Database: %DATABASE%
echo   Username: %USERNAME%

echo.
set /p CONFIRM="Do you want to run the migration? (y/N): "
if /i not "%CONFIRM%"=="y" (
    echo Migration cancelled.
    pause
    exit /b 0
)

echo.
echo Running migration...

REM Execute the SQL file
mysql -h %SERVER% -P %PORT% -u %USERNAME% -p%PASSWORD% %DATABASE% < %SQLFILE%

if errorlevel 1 (
    echo.
    echo === MIGRATION FAILED ===
    echo Check the error messages above
    pause
    exit /b 1
) else (
    echo.
    echo === MIGRATION SUCCESSFUL ===
    echo Database tables created and seeded successfully!
    echo.
    echo Next steps:
    echo 1. Your appsettings.json has been updated with correct connection string
    echo 2. Build and run your .NET application
    echo 3. Test the database connection
)

echo.
echo Migration completed!
pause 