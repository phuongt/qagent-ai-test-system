namespace QAgentWeb.Models.Extensions
{
    public static class TestingRuleExtensions
    {
        public static string GetCategoryDisplayName(this TestingRule rule)
        {
            return rule.RuleCategory switch
            {
                TestingRule.Categories.EquivalencePartitioning => "Equivalence Partitioning",
                TestingRule.Categories.BoundaryValueAnalysis => "Boundary Value Analysis", 
                TestingRule.Categories.DecisionTable => "Decision Table",
                TestingRule.Categories.StateTransition => "State Transition",
                TestingRule.Categories.ErrorGuessing => "Error Guessing",
                TestingRule.Categories.UseCase => "Use Case Testing",
                _ => rule.RuleCategory
            };
        }

        public static string GetPriorityBadgeClass(this TestingRule rule)
        {
            return rule.Priority switch
            {
                >= 9 => "bg-danger",
                >= 7 => "bg-warning", 
                >= 5 => "bg-primary",
                >= 3 => "bg-info",
                _ => "bg-secondary"
            };
        }
    }

    public static class RuleCategoryExtensions
    {
        public static string GetDisplayName(this RuleCategory category)
        {
            return category.CategoryId switch
            {
                "EP" => "Equivalence Partitioning",
                "BVA" => "Boundary Value Analysis",
                "DT" => "Decision Table",
                "ST" => "State Transition", 
                "EG" => "Error Guessing",
                "UCT" => "Use Case Testing",
                _ => category.CategoryName
            };
        }

        public static int GetActiveRulesCount(this RuleCategory category)
        {
            return category.TestingRules?.Count(r => r.IsActive) ?? 0;
        }
    }
} 