using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QAgentWeb.Data;
using QAgentWeb.Models;
using QAgentWeb.Models.DTOs;
using QAgentWeb.Services;
using System.Security.Claims;

namespace QAgentWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public AuthController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResultDto>> Login(LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto.Email, loginDto.Password);

                if (result.Success)
                {
                    // Update last login time
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
                    if (user != null)
                    {
                        user.LastLoginAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }

                    // Set session for backward compatibility
                    HttpContext.Session.SetString("UserId", result.User?.UserId ?? "");
                    HttpContext.Session.SetString("UserEmail", result.User?.Email ?? "");
                    HttpContext.Session.SetString("UserName", result.User?.FullName ?? "");

                    return Ok(result);
                }

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResultDto
                {
                    Success = false,
                    Message = "Login failed",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResultDto>> Register(RegisterDto registerDto)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return BadRequest(new AuthResultDto
                    {
                        Success = false,
                        Message = "User already exists",
                        Errors = new List<string> { "Email is already registered" }
                    });
                }

                var result = await _authService.RegisterAsync(
                    registerDto.Email,
                    registerDto.Password,
                    registerDto.FirstName,
                    registerDto.LastName,
                    registerDto.Phone,
                    registerDto.Organization,
                    registerDto.JobTitle);

                if (result.Success)
                {
                    // Set session for backward compatibility
                    HttpContext.Session.SetString("UserId", result.User?.UserId ?? "");
                    HttpContext.Session.SetString("UserEmail", result.User?.Email ?? "");
                    HttpContext.Session.SetString("UserName", result.User?.FullName ?? "");

                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResultDto
                {
                    Success = false,
                    Message = "Registration failed",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // POST: api/Auth/logout
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            try
            {
                // Clear session
                HttpContext.Session.Clear();

                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Logout failed", error = ex.Message });
            }
        }

        // GET: api/Auth/me
        [HttpGet("me")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Organization = user.Organization,
                    JobTitle = user.JobTitle,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    IsActive = user.IsActive,
                    Roles = new List<string> { "User" } // Default role
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user info", error = ex.Message });
            }
        }

        // PUT: api/Auth/me
        [HttpPut("me")]
        public async Task<ActionResult> UpdateCurrentUser(UpdateUserDto updateUserDto)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                user.FirstName = updateUserDto.FirstName;
                user.LastName = updateUserDto.LastName;
                user.Phone = updateUserDto.Phone;
                user.Organization = updateUserDto.Organization;
                user.JobTitle = updateUserDto.JobTitle;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to update user", error = ex.Message });
            }
        }

        // POST: api/Auth/change-password
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var result = await _authService.ChangePasswordAsync(
                    user.Email,
                    changePasswordDto.CurrentPassword,
                    changePasswordDto.NewPassword);

                if (result.Success)
                {
                    return Ok(new { message = "Password changed successfully" });
                }

                return BadRequest(new { message = "Password change failed", errors = result.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to change password", error = ex.Message });
            }
        }

        // POST: api/Auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email && !u.IsDeleted);
                if (user == null)
                {
                    // Don't reveal if user exists or not
                    return Ok(new { message = "If the email exists, a password reset link has been sent" });
                }

                var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);

                return Ok(new { message = "If the email exists, a password reset link has been sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to process forgot password request", error = ex.Message });
            }
        }

        // POST: api/Auth/reset-password
        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(
                    resetPasswordDto.Email,
                    resetPasswordDto.Token,
                    resetPasswordDto.NewPassword);

                if (result.Success)
                {
                    return Ok(new { message = "Password reset successfully" });
                }

                return BadRequest(new { message = "Password reset failed", errors = result.Errors });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to reset password", error = ex.Message });
            }
        }

        // GET: api/Auth/stats
        [HttpGet("stats")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var projects = await _context.Projects.Where(p => p.UserId == userId && !p.IsDeleted).ToListAsync();
                var screens = await _context.Screens.Where(s => s.UserId == userId && !s.IsDeleted).ToListAsync();
                var analyses = await _context.AnalysisLogs.Where(a => a.CreatedAt >= DateTime.UtcNow.AddDays(-30)).ToListAsync();

                var stats = new UserStatsDto
                {
                    TotalProjects = projects.Count,
                    TotalScreens = screens.Count,
                    CompletedAnalyses = analyses.Count(a => a.AnalysisStatus == "Completed"),
                    PendingAnalyses = analyses.Count(a => a.AnalysisStatus == "Processing"),
                    GeneratedChecklists = analyses.Count(a => a.AnalysisStatus == "Completed"), // Simplified
                    LastActivity = projects.Concat(screens.Cast<BaseEntity>()).Max(x => x.UpdatedAt),
                    RecentActivities = new List<RecentActivityDto>()
                };

                // Add recent activities
                var recentProjects = projects.OrderByDescending(p => p.UpdatedAt).Take(5);
                foreach (var project in recentProjects)
                {
                    stats.RecentActivities.Add(new RecentActivityDto
                    {
                        Type = "Project",
                        Description = $"Updated project: {project.Name}",
                        Timestamp = project.UpdatedAt,
                        ProjectId = project.Id
                    });
                }

                var recentScreens = screens.OrderByDescending(s => s.UpdatedAt).Take(5);
                foreach (var screen in recentScreens)
                {
                    stats.RecentActivities.Add(new RecentActivityDto
                    {
                        Type = "Screen",
                        Description = $"Updated screen: {screen.Name}",
                        Timestamp = screen.UpdatedAt,
                        ScreenId = screen.Id,
                        ProjectId = screen.ProjectId
                    });
                }

                stats.RecentActivities = stats.RecentActivities
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .ToList();

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to get user stats", error = ex.Message });
            }
        }

        // POST: api/Auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResultDto>> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(refreshTokenDto.Token, refreshTokenDto.RefreshToken);

                if (result.Success)
                {
                    return Ok(result);
                }

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new AuthResultDto
                {
                    Success = false,
                    Message = "Token refresh failed",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}