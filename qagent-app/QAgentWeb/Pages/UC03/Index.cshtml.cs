using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using QAgentWeb.Services;

namespace QAgentWeb.Pages.UC03
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IRuleEngineService _ruleEngineService;

        public IndexModel(ApplicationDbContext context, IRuleEngineService ruleEngineService)
        {
            _context = context;
            _ruleEngineService = ruleEngineService;
        }

        public IEnumerable<TestingRule> TestingRules { get; set; } = new List<TestingRule>();
        public IEnumerable<RuleCategory> RuleCategories { get; set; } = new List<RuleCategory>();
        public string? SelectedCategory { get; set; }
        public int TotalRules { get; set; }
        public int ActiveRules { get; set; }
        public int TotalApplications { get; set; }
        public double AverageEffectiveness { get; set; }

        public async Task OnGetAsync(string? category = null)
        {
            SelectedCategory = category;
            
            // Get rule categories
            RuleCategories = await _context.RuleCategories
                .Include(c => c.TestingRules)
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();

            // Get testing rules
            var rulesQuery = _context.TestingRules
                .Include(r => r.Category)
                .Include(r => r.ApplicationLogs)
                .Where(r => r.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                rulesQuery = rulesQuery.Where(r => r.RuleCategory == category);
            }

            TestingRules = await rulesQuery
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.RuleName)
                .ToListAsync();

            // Calculate statistics
            TotalRules = await _context.TestingRules.CountAsync(r => r.IsActive);
            ActiveRules = TotalRules; // All rules are active in this view
            TotalApplications = await _context.RuleApplicationLogs.CountAsync(l => l.WasSuccessful);
            
            var effectivenessScores = await _context.RuleApplicationLogs
                .Where(l => l.WasSuccessful && l.EffectivenessScore.HasValue)
                .Select(l => l.EffectivenessScore!.Value)
                .ToListAsync();
            
            AverageEffectiveness = effectivenessScores.Any() ? effectivenessScores.Average() : 0.0;
        }
    }
} 