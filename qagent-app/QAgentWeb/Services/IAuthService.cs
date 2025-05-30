using QAgentWeb.Models;
using QAgentWeb.Models.DTOs;

namespace QAgentWeb.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> ConfirmEmailAsync(string email, string token);
        Task<bool> SendPasswordResetEmailAsync(string email);
        Task<AuthResponse> ResetPasswordAsync(string email, string token, string newPassword);
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByEmailAsync(string email);
        string GenerateJwtToken(User user);
        bool ValidateJwtToken(string token, out string userId);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
} 