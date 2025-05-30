using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace QAgentWeb.Pages.UC09
{
    public class IndexModel : PageModel
    {
        private readonly IStringLocalizer<IndexModel> _localizer;

        public IndexModel(IStringLocalizer<IndexModel> localizer)
        {
            _localizer = localizer;
        }

        public void OnGet()
        {
            ViewData["Title"] = _localizer["UC09Title"]; // "UC09 - Gợi ý từ Vector Database";
        }
    }
} 