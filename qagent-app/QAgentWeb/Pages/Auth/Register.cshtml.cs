using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QAgentWeb.Models.DTOs;
using QAgentWeb.Services;

namespace QAgentWeb.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public RegisterRequest RegisterRequest { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
            // Check if user is already logged in
            if (HttpContext.Session.GetString("UserId") != null)
            {
                Response.Redirect("/dashboard");
                return;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var result = await _authService.RegisterAsync(RegisterRequest);

                if (result.Success)
                {
                    _logger.LogInformation("User registered successfully: {Email}", RegisterRequest.Email);

                    // Set success message and redirect to login
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToPage("/Auth/Login");
                }
                else
                {
                    ErrorMessage = result.Message;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration attempt for email: {Email}", RegisterRequest.Email);
                ErrorMessage = "Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại.";
                return Page();
            }
        }
    }
} 