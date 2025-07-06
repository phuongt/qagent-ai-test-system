using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using QAgentWeb.Models.DTOs;
using QAgentWeb.Services;

namespace QAgentWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRuleEngineService _ruleEngineService;

        public RulesController(ApplicationDbContext context, IRuleEngineService ruleEngineService)
        {
            _context = context;
            _ruleEngineService = ruleEngineService;
        }

        // GET: api/Rules
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TestingRuleDto>>> GetRules(
            [FromQuery] string? category = null,
            [FromQuery] string? ruleType = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? priority = null)
        {
            var query = _context.TestingRules
                .Include(r => r.Category)
                .Where(r => !r.IsDeleted);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(r => r.RuleCategory == category);
            }

            if (!string.IsNullOrEmpty(ruleType))
            {
                query = query.Where(r => r.RuleType == ruleType);
            }

            if (isActive.HasValue)
            {
                query = query.Where(r => r.IsActive == isActive.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(r => r.Priority == priority.Value);
            }

            var rules = await query
                .OrderBy(r => r.RuleCategory)
                .ThenBy(r => r.Priority)
                .Select(r => new TestingRuleDto
                {
                    Id = r.Id,
                    RuleId = r.RuleId,
                    RuleName = r.RuleName,
                    RuleCategory = r.RuleCategory,
                    RuleType = r.RuleType,
                    Description = r.Description,
                    DescriptionVi = r.DescriptionVi,
                    Priority = r.Priority,
                    IsActive = r.IsActive,
                    RuleTemplate = r.RuleTemplate,
                    Applicability = r.Applicability,
                    Examples = r.Examples,
                    Version = r.Version,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    RuleCategoryId = r.RuleCategoryId,
                    CategoryName = r.Category != null ? r.Category.CategoryName : null
                })
                .ToListAsync();

            return Ok(rules);
        }

        // GET: api/Rules/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TestingRuleDto>> GetRule(int id)
        {
            var rule = await _context.TestingRules
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (rule == null)
            {
                return NotFound();
            }

            var ruleDto = new TestingRuleDto
            {
                Id = rule.Id,
                RuleId = rule.RuleId,
                RuleName = rule.RuleName,
                RuleCategory = rule.RuleCategory,
                RuleType = rule.RuleType,
                Description = rule.Description,
                DescriptionVi = rule.DescriptionVi,
                Priority = rule.Priority,
                IsActive = rule.IsActive,
                RuleTemplate = rule.RuleTemplate,
                Applicability = rule.Applicability,
                Examples = rule.Examples,
                Version = rule.Version,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt,
                RuleCategoryId = rule.RuleCategoryId,
                CategoryName = rule.Category?.CategoryName
            };

            return Ok(ruleDto);
        }

        // POST: api/Rules
        [HttpPost]
        public async Task<ActionResult<TestingRuleDto>> CreateRule(CreateTestingRuleDto createRuleDto)
        {
            // Check if rule ID already exists
            if (await _context.TestingRules.AnyAsync(r => r.RuleId == createRuleDto.RuleId))
            {
                return BadRequest("Rule ID already exists");
            }

            // Verify category exists
            var category = await _context.RuleCategories.FindAsync(createRuleDto.RuleCategoryId);
            if (category == null || category.IsDeleted)
            {
                return BadRequest("Rule category not found");
            }

            var rule = new TestingRule
            {
                RuleId = createRuleDto.RuleId,
                RuleName = createRuleDto.RuleName,
                RuleCategory = createRuleDto.RuleCategory,
                RuleType = createRuleDto.RuleType,
                Description = createRuleDto.Description,
                DescriptionVi = createRuleDto.DescriptionVi,
                Priority = createRuleDto.Priority,
                IsActive = createRuleDto.IsActive,
                RuleTemplate = createRuleDto.RuleTemplate,
                Applicability = createRuleDto.Applicability,
                Examples = createRuleDto.Examples,
                Version = createRuleDto.Version,
                RuleCategoryId = createRuleDto.RuleCategoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TestingRules.Add(rule);
            await _context.SaveChangesAsync();

            var ruleDto = new TestingRuleDto
            {
                Id = rule.Id,
                RuleId = rule.RuleId,
                RuleName = rule.RuleName,
                RuleCategory = rule.RuleCategory,
                RuleType = rule.RuleType,
                Description = rule.Description,
                DescriptionVi = rule.DescriptionVi,
                Priority = rule.Priority,
                IsActive = rule.IsActive,
                RuleTemplate = rule.RuleTemplate,
                Applicability = rule.Applicability,
                Examples = rule.Examples,
                Version = rule.Version,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt,
                RuleCategoryId = rule.RuleCategoryId,
                CategoryName = category.CategoryName
            };

            return CreatedAtAction(nameof(GetRule), new { id = rule.Id }, ruleDto);
        }

        // PUT: api/Rules/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRule(int id, UpdateTestingRuleDto updateRuleDto)
        {
            var rule = await _context.TestingRules.FindAsync(id);
            if (rule == null || rule.IsDeleted)
            {
                return NotFound();
            }

            // Verify category exists
            var category = await _context.RuleCategories.FindAsync(updateRuleDto.RuleCategoryId);
            if (category == null || category.IsDeleted)
            {
                return BadRequest("Rule category not found");
            }

            rule.RuleName = updateRuleDto.RuleName;
            rule.RuleCategory = updateRuleDto.RuleCategory;
            rule.RuleType = updateRuleDto.RuleType;
            rule.Description = updateRuleDto.Description;
            rule.DescriptionVi = updateRuleDto.DescriptionVi;
            rule.Priority = updateRuleDto.Priority;
            rule.IsActive = updateRuleDto.IsActive;
            rule.RuleTemplate = updateRuleDto.RuleTemplate;
            rule.Applicability = updateRuleDto.Applicability;
            rule.Examples = updateRuleDto.Examples;
            rule.Version = updateRuleDto.Version;
            rule.RuleCategoryId = updateRuleDto.RuleCategoryId;
            rule.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RuleExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Rules/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRule(int id)
        {
            var rule = await _context.TestingRules.FindAsync(id);
            if (rule == null || rule.IsDeleted)
            {
                return NotFound();
            }

            rule.IsDeleted = true;
            rule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Rules/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<RuleCategoryDto>>> GetRuleCategories()
        {
            var categories = await _context.RuleCategories
                .Include(c => c.TestingRules)
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .Select(c => new RuleCategoryDto
                {
                    Id = c.Id,
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    ISTQBReference = c.ISTQBReference,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    RuleCount = c.TestingRules.Count(r => !r.IsDeleted)
                })
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/Rules/categories
        [HttpPost("categories")]
        public async Task<ActionResult<RuleCategoryDto>> CreateRuleCategory(CreateRuleCategoryDto createCategoryDto)
        {
            // Check if category ID already exists
            if (await _context.RuleCategories.AnyAsync(c => c.CategoryId == createCategoryDto.CategoryId))
            {
                return BadRequest("Category ID already exists");
            }

            var category = new RuleCategory
            {
                CategoryId = createCategoryDto.CategoryId,
                CategoryName = createCategoryDto.CategoryName,
                Description = createCategoryDto.Description,
                ISTQBReference = createCategoryDto.ISTQBReference,
                SortOrder = createCategoryDto.SortOrder,
                IsActive = createCategoryDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.RuleCategories.Add(category);
            await _context.SaveChangesAsync();

            var categoryDto = new RuleCategoryDto
            {
                Id = category.Id,
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                ISTQBReference = category.ISTQBReference,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                RuleCount = 0
            };

            return CreatedAtAction(nameof(GetRuleCategories), categoryDto);
        }

        // POST: api/Rules/apply
        [HttpPost("apply")]
        public async Task<ActionResult<ApplyRulesResultDto>> ApplyRules(ApplyRulesRequestDto applyRequest)
        {
            var screen = await _context.Screens
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == applyRequest.ScreenId && !s.IsDeleted);

            if (screen == null)
            {
                return NotFound("Screen not found");
            }

            try
            {
                var sessionId = Guid.NewGuid().ToString();
                var startedAt = DateTime.UtcNow;

                var result = new ApplyRulesResultDto
                {
                    SessionId = sessionId,
                    ScreenId = screen.Id,
                    ScreenName = screen.Name,
                    StartedAt = startedAt
                };

                // Apply rules using the rule engine service
                var appliedRules = await _ruleEngineService.ApplyRulesAsync(
                    screen.Id, 
                    applyRequest.BusinessDescription ?? "");

                // Convert to DTOs
                result.AppliedRules = appliedRules.Select(rule => new RuleApplicationDto
                {
                    SessionId = sessionId,
                    TestingRuleId = rule.TestingRuleId,
                    RuleName = rule.TestingRule?.RuleName,
                    ProjectId = rule.ProjectId,
                    ScreenId = rule.ScreenId,
                    WasSuccessful = rule.WasSuccessful,
                    AppliedContext = rule.AppliedContext,
                    AppliedAt = rule.AppliedAt,
                    UserId = rule.UserId,
                    Notes = rule.Notes,
                    ErrorMessage = rule.ErrorMessage
                }).ToList();

                result.TotalRulesApplied = result.AppliedRules.Count;
                result.SuccessfulApplications = result.AppliedRules.Count(r => r.WasSuccessful);
                result.FailedApplications = result.TotalRulesApplied - result.SuccessfulApplications;
                result.CompletedAt = DateTime.UtcNow;

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Rule application failed", error = ex.Message });
            }
        }

        // GET: api/Rules/applications
        [HttpGet("applications")]
        public async Task<ActionResult<IEnumerable<RuleApplicationDto>>> GetRuleApplications(
            [FromQuery] int? projectId = null,
            [FromQuery] int? screenId = null,
            [FromQuery] string? sessionId = null)
        {
            var query = _context.RuleApplicationLogs
                .Include(l => l.TestingRule)
                .Include(l => l.Project)
                .Include(l => l.Screen)
                .Where(l => !l.IsDeleted);

            if (projectId.HasValue)
            {
                query = query.Where(l => l.ProjectId == projectId.Value);
            }

            if (screenId.HasValue)
            {
                query = query.Where(l => l.ScreenId == screenId.Value);
            }

            if (!string.IsNullOrEmpty(sessionId))
            {
                query = query.Where(l => l.SessionId == sessionId);
            }

            var applications = await query
                .OrderByDescending(l => l.AppliedAt)
                .Select(l => new RuleApplicationDto
                {
                    Id = l.Id,
                    SessionId = l.SessionId,
                    TestingRuleId = l.TestingRuleId,
                    RuleName = l.TestingRule != null ? l.TestingRule.RuleName : null,
                    ProjectId = l.ProjectId,
                    ProjectName = l.Project != null ? l.Project.Name : null,
                    ScreenId = l.ScreenId,
                    ScreenName = l.Screen != null ? l.Screen.Name : null,
                    WasSuccessful = l.WasSuccessful,
                    AppliedContext = l.AppliedContext,
                    AppliedAt = l.AppliedAt,
                    UserId = l.UserId,
                    Notes = l.Notes,
                    ErrorMessage = l.ErrorMessage
                })
                .ToListAsync();

            return Ok(applications);
        }

        private bool RuleExists(int id)
        {
            return _context.TestingRules.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}