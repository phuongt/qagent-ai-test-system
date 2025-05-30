using Microsoft.EntityFrameworkCore;
using QAgentWeb.Models;
using System.Text.Json;

namespace QAgentWeb.Data.SeedData
{
    public static class ISTQBRulesSeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Seed Rule Categories
            if (!await context.RuleCategories.AnyAsync())
            {
                var categories = new List<RuleCategory>
                {
                    new RuleCategory
                    {
                        CategoryId = "EP",
                        CategoryName = "Equivalence Partitioning",
                        Description = "Technique chia dữ liệu input thành các partition tương đương, mỗi partition có cùng behavior",
                        ISTQBReference = "ISTQB Foundation Level 4.3.1",
                        SortOrder = 1,
                        IsActive = true
                    },
                    new RuleCategory
                    {
                        CategoryId = "BVA",
                        CategoryName = "Boundary Value Analysis",
                        Description = "Technique kiểm thử các giá trị biên của input domain",
                        ISTQBReference = "ISTQB Foundation Level 4.3.2",
                        SortOrder = 2,
                        IsActive = true
                    },
                    new RuleCategory
                    {
                        CategoryId = "DT",
                        CategoryName = "Decision Table Testing",
                        Description = "Technique kiểm thử logic business phức tạp sử dụng bảng quyết định",
                        ISTQBReference = "ISTQB Foundation Level 4.3.3",
                        SortOrder = 3,
                        IsActive = true
                    },
                    new RuleCategory
                    {
                        CategoryId = "ST",
                        CategoryName = "State Transition Testing",
                        Description = "Technique kiểm thử chuyển trạng thái của hệ thống",
                        ISTQBReference = "ISTQB Foundation Level 4.3.4",
                        SortOrder = 4,
                        IsActive = true
                    },
                    new RuleCategory
                    {
                        CategoryId = "EG",
                        CategoryName = "Error Guessing",
                        Description = "Technique dựa vào kinh nghiệm để đoán lỗi có thể xảy ra",
                        ISTQBReference = "ISTQB Foundation Level 4.4.4",
                        SortOrder = 5,
                        IsActive = true
                    },
                    new RuleCategory
                    {
                        CategoryId = "UCT",
                        CategoryName = "Use Case Testing",
                        Description = "Technique kiểm thử dựa trên use case specification",
                        ISTQBReference = "ISTQB Foundation Level 4.3.5",
                        SortOrder = 6,
                        IsActive = true
                    }
                };

                await context.RuleCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed Testing Rules
            if (!await context.TestingRules.AnyAsync())
            {
                var epCategory = await context.RuleCategories.FirstAsync(c => c.CategoryId == "EP");
                var bvaCategory = await context.RuleCategories.FirstAsync(c => c.CategoryId == "BVA");
                var dtCategory = await context.RuleCategories.FirstAsync(c => c.CategoryId == "DT");
                var stCategory = await context.RuleCategories.FirstAsync(c => c.CategoryId == "ST");
                var egCategory = await context.RuleCategories.FirstAsync(c => c.CategoryId == "EG");
                var uctCategory = await context.RuleCategories.FirstAsync(c => c.CategoryId == "UCT");

                var testingRules = new List<TestingRule>
                {
                    // Equivalence Partitioning Rules
                    new TestingRule
                    {
                        RuleId = "EP001",
                        RuleName = "Input Field Validation - Equivalence Classes",
                        RuleCategory = "EP",
                        RuleType = "input_validation",
                        Description = "Create test cases for valid and invalid equivalence classes of input fields",
                        DescriptionVi = "Tạo test case cho các lớp tương đương hợp lệ và không hợp lệ của input field",
                        Applicability = JsonSerializer.Serialize(new ApplicabilityConfig
                        {
                            ScreenTypes = new List<string> { "Form", "Login", "Search" },
                            UIElementTypes = new List<string> { "input", "textarea", "dropdown" },
                            BusinessContexts = new List<string> { "form_validation", "data_entry" }
                        }),
                        RuleTemplate = "For input field {field_name}: Create test cases for {valid_partitions} valid partitions and {invalid_partitions} invalid partitions",
                        Examples = JsonSerializer.Serialize(new List<RuleExample>
                        {
                            new RuleExample { Title = "Age Field", Description = "Valid: 18-65, Invalid: <18, >65, non-numeric" },
                            new RuleExample { Title = "Email Field", Description = "Valid: proper format, Invalid: missing @, invalid domain" }
                        }),
                        Priority = 8,
                        IsActive = true,
                        Version = "1.0",
                        RuleCategoryId = epCategory.Id
                    },
                    new TestingRule
                    {
                        RuleId = "EP002", 
                        RuleName = "Dropdown Selection - Valid Options",
                        RuleCategory = "EP",
                        RuleType = "input_validation",
                        Description = "Test valid selections from dropdown/select lists",
                        DescriptionVi = "Kiểm thử các lựa chọn hợp lệ từ dropdown/select list",
                        Applicability = JsonSerializer.Serialize(new ApplicabilityConfig
                        {
                            ScreenTypes = new List<string> { "Form" },
                            UIElementTypes = new List<string> { "dropdown", "select", "combobox" },
                            BusinessContexts = new List<string> { "form_validation" }
                        }),
                        RuleTemplate = "Test all available options in dropdown {dropdown_name}, verify selected value is properly handled",
                        Priority = 7,
                        IsActive = true,
                        Version = "1.0",
                        RuleCategoryId = epCategory.Id
                    },

                    // Boundary Value Analysis Rules
                    new TestingRule
                    {
                        RuleId = "BVA001",
                        RuleName = "Numeric Input Boundaries",
                        RuleCategory = "BVA",
                        RuleType = "boundary_check",
                        Description = "Test boundary values for numeric input fields",
                        DescriptionVi = "Kiểm thử giá trị biên cho input field số",
                        Applicability = JsonSerializer.Serialize(new ApplicabilityConfig
                        {
                            ScreenTypes = new List<string> { "Form" },
                            UIElementTypes = new List<string> { "input", "number" },
                            BusinessContexts = new List<string> { "form_validation", "calculation" }
                        }),
                        RuleTemplate = "For numeric field {field_name} with range [{min}-{max}]: Test {min-1}, {min}, {min+1}, {max-1}, {max}, {max+1}",
                        Examples = JsonSerializer.Serialize(new List<RuleExample>
                        {
                            new RuleExample { Title = "Quantity Field (1-100)", Description = "Test: 0, 1, 2, 99, 100, 101" },
                            new RuleExample { Title = "Price Field (0.01-9999.99)", Description = "Test: 0.00, 0.01, 0.02, 9999.98, 9999.99, 10000.00" }
                        }),
                        Priority = 9,
                        IsActive = true,
                        Version = "1.0", 
                        RuleCategoryId = bvaCategory.Id
                    },
                    new TestingRule
                    {
                        RuleId = "BVA002",
                        RuleName = "String Length Boundaries",
                        RuleCategory = "BVA",
                        RuleType = "boundary_check",
                        Description = "Test boundary values for string length validation",
                        DescriptionVi = "Kiểm thử giá trị biên cho validation độ dài chuỗi",
                        Applicability = JsonSerializer.Serialize(new ApplicabilityConfig
                        {
                            ScreenTypes = new List<string> { "Form" },
                            UIElementTypes = new List<string> { "input", "textarea" },
                            BusinessContexts = new List<string> { "form_validation" }
                        }),
                        RuleTemplate = "For string field {field_name} with length [{min}-{max}]: Test strings of length {min-1}, {min}, {min+1}, {max-1}, {max}, {max+1}",
                        Priority = 8,
                        IsActive = true,
                        Version = "1.0",
                        RuleCategoryId = bvaCategory.Id
                    },

                    // Decision Table Rules
                    new TestingRule
                    {
                        RuleId = "DT001",
                        RuleName = "Complex Business Logic - Decision Table",
                        RuleCategory = "DT",
                        RuleType = "logic_flow",
                        Description = "Create decision table for complex business rules with multiple conditions",
                        DescriptionVi = "Tạo bảng quyết định cho business rule phức tạp với nhiều điều kiện",
                        Applicability = JsonSerializer.Serialize(new ApplicabilityConfig
                        {
                            ScreenTypes = new List<string> { "Form", "Dashboard" },
                            UIElementTypes = new List<string> { "checkbox", "radio", "dropdown" },
                            BusinessContexts = new List<string> { "calculation", "approval_workflow" }
                        }),
                        RuleTemplate = "Create decision table with conditions: {conditions} and actions: {actions}. Test all valid combinations",
                        Priority = 7,
                        IsActive = true,
                        Version = "1.0",
                        RuleCategoryId = dtCategory.Id
                    },

                    // State Transition Rules
                    new TestingRule
                    {
                        RuleId = "ST001",
                        RuleName = "User Authentication States",
                        RuleCategory = "ST",
                        RuleType = "logic_flow",
                        Description = "Test state transitions in user authentication flow",
                        DescriptionVi = "Kiểm thử chuyển trạng thái trong luồng xác thực user",
                        Applicability = JsonSerializer.Serialize(new ApplicabilityConfig
                        {
                            ScreenTypes = new List<string> { "Login", "Dashboard" },
                            UIElementTypes = new List<string> { "button", "link" },
                            BusinessContexts = new List<string> { "authentication", "session_management" }
                        }),
                        RuleTemplate = "Test transitions: {initial_state} → {event} → {target_state}. Verify invalid transitions are blocked",
                        Priority = 9,
                        IsActive = true,
                        Version = "1.0",
                        RuleCategoryId = stCategory.Id
                    },

                    // Error Guessing Rules
                    new TestingRule
                    {
                        RuleId = "EG001",
                        RuleName = "Common Input Errors",
                        RuleCategory = "EG",
                        RuleType = "error_handling",
                        Description = "Test common input errors based on experience",
                        DescriptionVi = "Kiểm thử các lỗi input phổ biến dựa trên kinh nghiệm",
                        Applicability = JsonSerializer.Serialize(new ApplicabilityConfig
                        {
                            ScreenTypes = new List<string> { "Form", "Login" },
                            UIElementTypes = new List<string> { "input", "textarea" },
                            BusinessContexts = new List<string> { "form_validation", "error_handling" }
                        }),
                        RuleTemplate = "Test common error scenarios: {error_types}. Verify proper error messages and handling",
                        Examples = JsonSerializer.Serialize(new List<RuleExample>
                        {
                            new RuleExample { Title = "Special Characters", Description = "Test: <script>, SQL injection attempts, Unicode chars" },
                            new RuleExample { Title = "Null/Empty Values", Description = "Test: null, empty string, whitespace only" }
                        }),
                        Priority = 6,
                        IsActive = true,
                        Version = "1.0",
                        RuleCategoryId = egCategory.Id
                    },

                    // Use Case Testing Rules
                    new TestingRule
                    {
                        RuleId = "UCT001",
                        RuleName = "End-to-End User Workflow",
                        RuleCategory = "UCT",
                        RuleType = "logic_flow",
                        Description = "Test complete user workflow from start to finish",
                        DescriptionVi = "Kiểm thử workflow user hoàn chỉnh từ đầu đến cuối",
                        Applicability = JsonSerializer.Serialize(new ApplicabilityConfig
                        {
                            ScreenTypes = new List<string> { "Form", "Dashboard", "List" },
                            UIElementTypes = new List<string> { "button", "link", "navigation" },
                            BusinessContexts = new List<string> { "user_workflow", "business_process" }
                        }),
                        RuleTemplate = "Test use case: {use_case_name} with main flow: {main_steps} and alternative flows: {alt_flows}",
                        Priority = 8,
                        IsActive = true,
                        Version = "1.0",
                        RuleCategoryId = uctCategory.Id
                    }
                };

                await context.TestingRules.AddRangeAsync(testingRules);
                await context.SaveChangesAsync();
            }
        }
    }
}

// Supporting classes for JSON serialization
public class ApplicabilityConfig
{
    public List<string> ScreenTypes { get; set; } = new List<string>();
    public List<string> UIElementTypes { get; set; } = new List<string>();
    public List<string> BusinessContexts { get; set; } = new List<string>();
}

public class RuleExample
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 