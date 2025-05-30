using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using System.Text.Json;

namespace QAgentWeb.Services
{
    public interface IRuleEngineService
    {
        Task<IEnumerable<TestingRule>> GetApplicableRulesAsync(string screenType, List<string>? uiElements = null, List<string>? businessContexts = null);
        Task<IEnumerable<TestingRule>> GetRulesByCategoryAsync(string category);
        Task<TestingRule?> GetRuleByIdAsync(string ruleId);
        Task<RuleApplicationLog> ApplyRuleAsync(int ruleId, int? projectId = null, int? screenId = null, string? context = null);
        Task<IEnumerable<RuleApplicationLog>> GetApplicationHistoryAsync(int? projectId = null, int? screenId = null);
        Task<double> CalculateRuleEffectivenessAsync(int ruleId);
        Task<IEnumerable<TestingRule>> GetRecommendedRulesAsync(Screen screen);
    }

    public class RuleEngineService : IRuleEngineService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RuleEngineService> _logger;

        public RuleEngineService(ApplicationDbContext context, ILogger<RuleEngineService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TestingRule>> GetApplicableRulesAsync(string screenType, List<string>? uiElements = null, List<string>? businessContexts = null)
        {
            try
            {
                var query = _context.TestingRules
                    .Include(r => r.Category)
                    .Where(r => r.IsActive);

                var allRules = await query.ToListAsync();
                
                // Filter rules based on applicability
                var applicableRules = allRules.Where(rule =>
                {
                    if (string.IsNullOrEmpty(rule.Applicability))
                        return true; // Default to applicable if no specific rules

                    try
                    {
                        var config = JsonSerializer.Deserialize<ApplicabilityConfig>(rule.Applicability);
                        
                        // Check screen type
                        if (config?.ScreenTypes != null && config.ScreenTypes.Any())
                        {
                            if (!config.ScreenTypes.Contains(screenType))
                                return false;
                        }

                        // Check UI elements
                        if (uiElements != null && config?.UIElementTypes != null && config.UIElementTypes.Any())
                        {
                            if (!uiElements.Any(element => config.UIElementTypes.Contains(element)))
                                return false;
                        }

                        // Check business contexts
                        if (businessContexts != null && config?.BusinessContexts != null && config.BusinessContexts.Any())
                        {
                            if (!businessContexts.Any(context => config.BusinessContexts.Contains(context)))
                                return false;
                        }

                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to parse applicability config for rule {rule.RuleId}: {ex.Message}");
                        return true; // Default to applicable on parsing error
                    }
                })
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.RuleName);

                return applicableRules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting applicable rules for screen type: {ScreenType}", screenType);
                return Enumerable.Empty<TestingRule>();
            }
        }

        public async Task<IEnumerable<TestingRule>> GetRulesByCategoryAsync(string category)
        {
            try
            {
                return await _context.TestingRules
                    .Include(r => r.Category)
                    .Where(r => r.RuleCategory == category && r.IsActive)
                    .OrderBy(r => r.Priority)
                    .ThenBy(r => r.RuleName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rules by category: {Category}", category);
                return Enumerable.Empty<TestingRule>();
            }
        }

        public async Task<TestingRule?> GetRuleByIdAsync(string ruleId)
        {
            try
            {
                return await _context.TestingRules
                    .Include(r => r.Category)
                    .FirstOrDefaultAsync(r => r.RuleId == ruleId && r.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rule by ID: {RuleId}", ruleId);
                return null;
            }
        }

        public async Task<RuleApplicationLog> ApplyRuleAsync(int ruleId, int? projectId = null, int? screenId = null, string? context = null)
        {
            var sessionId = Guid.NewGuid().ToString("N")[..16];
            
            try
            {
                var rule = await _context.TestingRules.FindAsync(ruleId);
                if (rule == null)
                {
                    throw new ArgumentException($"Rule with ID {ruleId} not found");
                }

                // Create application log
                var log = new RuleApplicationLog
                {
                    SessionId = sessionId,
                    ProjectId = projectId,
                    ScreenId = screenId,
                    TestingRuleId = ruleId,
                    AppliedContext = context,
                    AppliedAt = DateTime.UtcNow
                };

                // Simulate rule application (in real implementation, this would generate actual checklist items)
                var checklistItems = SimulateChecklistGeneration(rule, context);
                var effectiveness = CalculateEffectiveness(rule, checklistItems);

                log.MarkAsSuccessful(checklistItems, effectiveness, context);

                _context.RuleApplicationLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully applied rule {RuleId} in session {SessionId}", ruleId, sessionId);
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying rule {RuleId} in session {SessionId}", ruleId, sessionId);
                
                var errorLog = new RuleApplicationLog
                {
                    SessionId = sessionId,
                    ProjectId = projectId,
                    ScreenId = screenId,
                    TestingRuleId = ruleId,
                    AppliedAt = DateTime.UtcNow
                };
                errorLog.MarkAsFailed(ex.Message);

                _context.RuleApplicationLogs.Add(errorLog);
                await _context.SaveChangesAsync();

                return errorLog;
            }
        }

        public async Task<IEnumerable<RuleApplicationLog>> GetApplicationHistoryAsync(int? projectId = null, int? screenId = null)
        {
            try
            {
                var query = _context.RuleApplicationLogs
                    .Include(l => l.TestingRule)
                    .Include(l => l.Project)
                    .Include(l => l.Screen)
                    .OrderByDescending(l => l.AppliedAt);

                if (projectId.HasValue)
                    query = (IOrderedQueryable<RuleApplicationLog>)query.Where(l => l.ProjectId == projectId);

                if (screenId.HasValue)
                    query = (IOrderedQueryable<RuleApplicationLog>)query.Where(l => l.ScreenId == screenId);

                return await query.Take(100).ToListAsync(); // Limit to recent 100 entries
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting application history for project: {ProjectId}, screen: {ScreenId}", projectId, screenId);
                return Enumerable.Empty<RuleApplicationLog>();
            }
        }

        public async Task<double> CalculateRuleEffectivenessAsync(int ruleId)
        {
            try
            {
                var logs = await _context.RuleApplicationLogs
                    .Where(l => l.TestingRuleId == ruleId && l.WasSuccessful && l.EffectivenessScore.HasValue)
                    .ToListAsync();

                if (!logs.Any())
                    return 0.0;

                return logs.Average(l => l.EffectivenessScore!.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating effectiveness for rule: {RuleId}", ruleId);
                return 0.0;
            }
        }

        public async Task<IEnumerable<TestingRule>> GetRecommendedRulesAsync(Screen screen)
        {
            try
            {
                // Get UI elements and business contexts from screen analysis
                var uiElements = ExtractUIElementsFromScreen(screen);
                var businessContexts = ExtractBusinessContextsFromScreen(screen);

                // Get applicable rules
                var applicableRules = await GetApplicableRulesAsync(screen.ScreenType, uiElements, businessContexts);

                // Apply ML-based recommendations (simplified implementation)
                var recommendedRules = applicableRules
                    .Take(10) // Top 10 recommendations
                    .ToList();

                return recommendedRules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended rules for screen: {ScreenId}", screen.Id);
                return Enumerable.Empty<TestingRule>();
            }
        }

        // Helper methods
        private int SimulateChecklistGeneration(TestingRule rule, string? context)
        {
            // Simulate checklist item generation based on rule category
            return rule.RuleCategory switch
            {
                TestingRule.Categories.EquivalencePartitioning => Random.Shared.Next(3, 8),
                TestingRule.Categories.BoundaryValueAnalysis => Random.Shared.Next(5, 12),
                TestingRule.Categories.DecisionTable => Random.Shared.Next(4, 10),
                TestingRule.Categories.StateTransition => Random.Shared.Next(6, 15),
                TestingRule.Categories.ErrorGuessing => Random.Shared.Next(2, 6),
                TestingRule.Categories.UseCase => Random.Shared.Next(8, 20),
                _ => Random.Shared.Next(2, 8)
            };
        }

        private double CalculateEffectiveness(TestingRule rule, int checklistItems)
        {
            // Simulate effectiveness calculation based on rule priority and generated items
            var baseScore = rule.Priority * 0.8;
            var itemsScore = Math.Min(checklistItems * 0.3, 3.0);
            var randomFactor = (Random.Shared.NextDouble() - 0.5) * 2.0; // ±1.0

            return Math.Max(0.0, Math.Min(10.0, baseScore + itemsScore + randomFactor));
        }

        private List<string> ExtractUIElementsFromScreen(Screen screen)
        {
            // Simplified extraction based on screen type
            return screen.ScreenType switch
            {
                Screen.ScreenTypes.Form => new List<string> { "input", "button", "dropdown", "checkbox", "textarea" },
                Screen.ScreenTypes.Login => new List<string> { "input", "button", "link" },
                Screen.ScreenTypes.Dashboard => new List<string> { "chart", "card", "navigation", "button" },
                Screen.ScreenTypes.List => new List<string> { "table", "pagination", "filter", "search" },
                Screen.ScreenTypes.Detail => new List<string> { "text", "image", "button", "link" },
                _ => new List<string> { "button", "link", "text" }
            };
        }

        private List<string> ExtractBusinessContextsFromScreen(Screen screen)
        {
            // Simplified extraction based on screen description and type
            var contexts = new List<string>();

            if (screen.Description.ToLower().Contains("form") || screen.ScreenType == Screen.ScreenTypes.Form)
                contexts.Add("form_validation");

            if (screen.Description.ToLower().Contains("search") || screen.ScreenType == Screen.ScreenTypes.List)
                contexts.Add("search");

            if (screen.Description.ToLower().Contains("calculate") || screen.Description.ToLower().Contains("calculation"))
                contexts.Add("calculation");

            if (screen.ScreenType == Screen.ScreenTypes.Login)
                contexts.Add("authentication");

            return contexts;
        }
    }
} 