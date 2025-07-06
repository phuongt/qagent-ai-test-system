# Oracle Database Test System - QAgent

## Overview

This Oracle database test system provides comprehensive testing capabilities for the QAgent application with Oracle database integration. The system includes automated test data creation, record count verification, performance testing, and detailed reporting.

## Features

### ✅ **Complete Test Coverage**
- **Connection Testing**: Verifies Oracle database connectivity
- **Schema Setup**: Creates and validates database schema
- **Test Data Creation**: Generates realistic test data with proper relationships
- **Record Count Verification**: Ensures exact expected record counts
- **Data Relationship Testing**: Validates foreign key relationships
- **Performance Testing**: Measures query execution times

### ✅ **Non-Hardcoded Configuration**
- Configurable through environment variables
- Command-line parameter support
- JSON configuration file support
- Dynamic connection string building

### ✅ **Expected Record Counts**
| Table | Expected Count | Description |
|-------|----------------|-------------|
| Users | 5 | Test user accounts |
| Projects | 10 | Test projects |
| Screens | 50 | UI screens for testing |
| QAgentTasks | 25 | Task management records |
| TestingRules | 100 | ISTQB testing rules |
| RuleCategories | 10 | Rule category definitions |
| UploadSessions | 5 | File upload sessions |

### ✅ **Oracle-Specific Features**
- Oracle sequences for identity columns
- VARCHAR2 data types for string fields
- CLOB fields for large text data
- TIMESTAMP columns for date/time data
- Oracle connection pooling and retry logic

## Files Structure

```
├── qagent-app/QAgentWeb/
│   ├── Tests/
│   │   ├── OracleDbTestConfig.cs      # Test configuration
│   │   ├── OracleDbTest.cs            # Main test logic
│   │   └── OracleTestRunner.cs        # Test runner
│   ├── appsettings.Test.json          # Test configuration
│   ├── QAgentWeb.csproj               # Updated with Oracle packages
│   └── Program.cs                     # Modified to support test runner
├── ORACLE_DB_TEST.ps1                 # PowerShell test runner
├── ORACLE_DB_TEST.sh                  # Bash test runner
└── ORACLE_DB_TEST_README.md           # This file
```

## Installation & Prerequisites

### Requirements
- .NET 8.0 SDK
- Oracle Database 12c or higher
- Oracle.EntityFrameworkCore package
- Oracle.ManagedDataAccess.Core package

### Package Installation
The following packages are automatically included:
```xml
<PackageReference Include="Oracle.EntityFrameworkCore" Version="8.23.60" />
<PackageReference Include="Oracle.ManagedDataAccess.Core" Version="23.6.1" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

## Configuration

### 1. Environment Variables
```bash
export ORACLE_HOST="localhost"
export ORACLE_PORT="1521"
export ORACLE_SERVICE_NAME="XEPDB1"
export ORACLE_USER="QAGENT_USER"
export ORACLE_PASSWORD="TestPassword123"
```

### 2. Configuration File (appsettings.Test.json)
```json
{
  "ConnectionStrings": {
    "OracleTestConnection": "User Id=QAGENT_USER;Password=TestPassword123;Data Source=localhost:1521/XEPDB1;"
  },
  "OracleTest": {
    "ExpectedProjectCount": 10,
    "ExpectedScreenCount": 50,
    "ExpectedUserCount": 5,
    "ExpectedTaskCount": 25,
    "ExpectedRuleCount": 100,
    "EnableAutoCleanup": true,
    "CommandTimeoutSeconds": 120
  }
}
```

### 3. Command Line Parameters
```bash
# PowerShell
./ORACLE_DB_TEST.ps1 -OracleHost "oracle.example.com" -OraclePort 1521 -Verbose

# Bash
./ORACLE_DB_TEST.sh --host "oracle.example.com" --port 1521 --verbose
```

## Usage

### PowerShell (Windows)
```powershell
# Basic test
./ORACLE_DB_TEST.ps1

# Verbose with cleanup
./ORACLE_DB_TEST.ps1 -Verbose -Cleanup

# Custom timeout and no cleanup
./ORACLE_DB_TEST.ps1 -Timeout 60 -NoCleanup

# Custom Oracle connection
./ORACLE_DB_TEST.ps1 -OracleHost "oracle.company.com" -OraclePort 1521 -OracleUser "test_user"
```

### Bash (Linux/macOS)
```bash
# Basic test
./ORACLE_DB_TEST.sh

# Verbose with cleanup
./ORACLE_DB_TEST.sh --verbose --cleanup

# Custom timeout and no cleanup
./ORACLE_DB_TEST.sh --timeout 60 --no-cleanup

# Custom Oracle connection
./ORACLE_DB_TEST.sh --host "oracle.company.com" --port 1521 --user "test_user"
```

### .NET Direct
```bash
cd qagent-app/QAgentWeb
dotnet run test-oracle --verbose --cleanup
```

## Test Process

### 1. Prerequisites Check
- ✅ Verifies .NET 8.0 SDK installation
- ✅ Validates project files existence
- ✅ Confirms Oracle client availability

### 2. Database Setup
- ✅ Tests Oracle database connection
- ✅ Creates fresh schema (drops existing if needed)
- ✅ Applies Oracle-specific configurations

### 3. Test Data Creation
- ✅ Creates **5 Users** with encrypted passwords
- ✅ Creates **10 Rule Categories** (UI, Functional, Security, etc.)
- ✅ Creates **100 Testing Rules** distributed across categories
- ✅ Creates **10 Projects** with various domains and statuses
- ✅ Creates **5 Upload Sessions** linked to projects
- ✅ Creates **50 Screens** with different types and priorities
- ✅ Creates **25 QAgent Tasks** with proper relationships

### 4. Record Count Verification
- ✅ Counts records in each table
- ✅ Compares with expected counts
- ✅ Reports any discrepancies
- ✅ Fails test if counts don't match

### 5. Data Relationship Testing
- ✅ Tests User-Project relationships
- ✅ Tests Project-Screen relationships
- ✅ Tests Category-Rule relationships
- ✅ Validates foreign key constraints

### 6. Performance Testing
- ✅ Simple count queries
- ✅ Complex join queries
- ✅ Aggregation queries
- ✅ Performance threshold validation

### 7. Cleanup & Reporting
- ✅ Removes test data (if enabled)
- ✅ Generates detailed report
- ✅ Provides recommendations

## Error Handling

### Common Issues & Solutions

#### 1. Connection Failures
```
Error: Oracle connection failed
Solution: 
- Check Oracle database is running
- Verify connection string
- Ensure network connectivity
- Check user permissions
```

#### 2. Schema Creation Failures
```
Error: Database setup failed
Solution:
- Verify user has CREATE TABLE privileges
- Check tablespace availability
- Ensure sufficient database space
```

#### 3. Record Count Mismatches
```
Error: Expected 10 Projects, got 8
Solution:
- Check for database constraints
- Verify transaction completion
- Review error logs for failed insertions
```

#### 4. Performance Issues
```
Warning: Query execution time > 5 seconds
Solution:
- Create appropriate indexes
- Optimize query execution plans
- Consider database tuning
```

## Test Reports

### Report Location
```
Oracle_Test_Report_YYYYMMDD_HHMMSS.md
```

### Report Contents
- **Test Summary**: Overall pass/fail status
- **Configuration**: Database and test settings
- **Record Counts**: Expected vs actual counts
- **Performance Metrics**: Query execution times
- **Recommendations**: Optimization suggestions
- **Next Steps**: Deployment recommendations

### Sample Report Structure
```markdown
# Oracle Database Test Report
Generated: 2025-07-06 23:18:13

## Test Summary
- Status: PASSED
- Total Errors: 0
- Total Warnings: 0

## Record Count Verification
| Table | Expected | Actual | Status |
|-------|----------|---------|--------|
| Users | 5 | 5 | ✓ PASS |
| Projects | 10 | 10 | ✓ PASS |
...
```

## Advanced Configuration

### Custom Test Data Volumes
```json
{
  "OracleTest": {
    "ExpectedProjectCount": 20,    // Custom project count
    "ExpectedScreenCount": 100,    // Custom screen count
    "ExpectedUserCount": 10,       // Custom user count
    "ExpectedTaskCount": 50,       // Custom task count
    "ExpectedRuleCount": 200       // Custom rule count
  }
}
```

### Performance Thresholds
```json
{
  "OracleTest": {
    "MaxQueryTimeSeconds": 3,      // Query timeout
    "MaxConnectionTimeSeconds": 10, // Connection timeout
    "CommandTimeoutSeconds": 300   // Command timeout
  }
}
```

### Cleanup Configuration
```json
{
  "OracleTest": {
    "EnableAutoCleanup": true,     // Auto cleanup after test
    "PreserveTestData": false,     // Keep test data for analysis
    "CleanupTimeout": 60           // Cleanup timeout
  }
}
```

## Best Practices

### 1. Test Environment Setup
- Use dedicated test database
- Configure proper user permissions
- Set up connection pooling
- Enable performance monitoring

### 2. Test Data Management
- Use realistic test data volumes
- Maintain data consistency
- Clean up after tests
- Monitor resource usage

### 3. Performance Optimization
- Create appropriate indexes
- Monitor query execution plans
- Use connection pooling
- Implement retry logic

### 4. Error Handling
- Implement comprehensive logging
- Handle transient failures
- Provide meaningful error messages
- Support retry mechanisms

## Troubleshooting

### Debug Mode
```bash
# Enable detailed logging
./ORACLE_DB_TEST.sh --verbose

# Preserve test data for analysis
./ORACLE_DB_TEST.sh --no-cleanup
```

### Manual Testing
```bash
# Test specific components
cd qagent-app/QAgentWeb
dotnet run test-oracle --connection "User Id=test;Password=test;Data Source=localhost:1521/XEPDB1;"
```

### Log Analysis
```bash
# Check application logs
tail -f Oracle_Test_Report_*.md

# Review error details
grep -i "error\|fail" Oracle_Test_Report_*.md
```

## Security Considerations

### 1. Credentials Management
- Use environment variables for sensitive data
- Avoid hardcoding passwords
- Implement secure credential storage
- Use connection string encryption

### 2. Database Security
- Create dedicated test user
- Limit database permissions
- Use encrypted connections
- Monitor database access

### 3. Test Data Security
- Use anonymized test data
- Avoid production data in tests
- Implement data masking
- Secure test environments

## Deployment & CI/CD

### Integration with CI/CD
```yaml
# GitHub Actions example
- name: Run Oracle DB Tests
  run: ./ORACLE_DB_TEST.sh --timeout 20 --no-cleanup
  env:
    ORACLE_HOST: ${{ secrets.ORACLE_HOST }}
    ORACLE_USER: ${{ secrets.ORACLE_USER }}
    ORACLE_PASSWORD: ${{ secrets.ORACLE_PASSWORD }}
```

### Docker Support
```dockerfile
# Dockerfile example
FROM mcr.microsoft.com/dotnet/sdk:8.0
COPY . /app
WORKDIR /app
RUN chmod +x ORACLE_DB_TEST.sh
CMD ["./ORACLE_DB_TEST.sh"]
```

## Support & Maintenance

### Regular Maintenance
- Update Oracle client drivers
- Review performance metrics
- Update test data volumes
- Monitor error patterns

### Support Resources
- Oracle documentation
- .NET Entity Framework guides
- QAgent application documentation
- Test framework best practices

## Conclusion

This Oracle database test system provides comprehensive testing capabilities for the QAgent application with Oracle database integration. The system ensures data integrity, validates expected record counts, and provides detailed reporting for analysis and troubleshooting.

The test system is designed to be:
- **Configurable**: No hardcoded values
- **Reliable**: Comprehensive error handling
- **Scalable**: Supports various test data volumes
- **Maintainable**: Clear documentation and logging
- **Secure**: Proper credential management

For questions or support, please refer to the QAgent documentation or contact the development team.