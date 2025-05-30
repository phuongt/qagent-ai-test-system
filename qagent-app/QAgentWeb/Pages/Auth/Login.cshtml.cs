using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QAgentWeb.Models.DTOs;
using QAgentWeb.Services;

namespace QAgentWeb.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            // Check if user is already logged in
            if (HttpContext.Session.GetString("UserId") != null)
            {
                Response.Redirect("/dashboard");
                return;
            }

            // Check for success message from registration
            if (TempData["SuccessMessage"] != null)
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString();
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
                var result = await _authService.LoginAsync(LoginRequest);

                if (result.Success)
                {
                    // Store user info in session
                    if (result.User != null && result.Token != null)
                    {
                        HttpContext.Session.SetString("UserId", result.User.UserId);
                        HttpContext.Session.SetString("UserEmail", result.User.Email);
                        HttpContext.Session.SetString("UserName", result.User.FullName);
                        HttpContext.Session.SetString("AuthToken", result.Token);
                    }

                    _logger.LogInformation("User logged in successfully: {Email}", result.User?.Email);

                    // Redirect to dashboard or return URL
                    var returnUrl = Request.Query["returnUrl"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToPage("/Dashboard/Index");
                }
                else
                {
                    ErrorMessage = result.Message;
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login attempt for email: {Email}", LoginRequest.Email);
                ErrorMessage = "Đã xảy ra lỗi trong quá trình đăng nhập. Vui lòng thử lại.";
                return Page();
            }
        }
    }
} 