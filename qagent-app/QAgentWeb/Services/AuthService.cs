using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QAgentWeb.Data;
using QAgentWeb.Models;
using QAgentWeb.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QAgentWeb.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không đúng"
                    };
                }

                if (!user.IsActive)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Tài khoản đã bị vô hiệu hóa"
                    };
                }

                if (!VerifyPassword(request.Password, user.PasswordHash))
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không đúng"
                    };
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    User = new UserInfo
                    {
                        Id = user.Id.ToString(),
                        UserId = user.UserId,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        IsEmailConfirmed = user.IsEmailConfirmed
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng nhập"
                };
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Check if user already exists
                var existingUser = await GetUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    };
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email.ToLowerInvariant(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PasswordHash = HashPassword(request.Password),
                    IsEmailConfirmed = false,
                    EmailConfirmationToken = GenerateRandomToken(),
                    EmailConfirmationTokenExpiry = DateTime.UtcNow.AddDays(1),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // TODO: Send confirmation email
                _logger.LogInformation("User registered successfully: {Email}", user.Email);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.",
                    User = new UserInfo
                    {
                        Id = user.Id.ToString(),
                        UserId = user.UserId,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        IsEmailConfirmed = user.IsEmailConfirmed
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng ký"
                };
            }
        }

        public async Task<AuthResponse> ConfirmEmailAsync(string email, string token)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Người dùng không tồn tại"
                    };
                }

                if (user.IsEmailConfirmed)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Email đã được xác thực"
                    };
                }

                if (user.EmailConfirmationToken != token || 
                    user.EmailConfirmationTokenExpiry == null || 
                    user.EmailConfirmationTokenExpiry < DateTime.UtcNow)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Token xác thực không hợp lệ hoặc đã hết hạn"
                    };
                }

                user.IsEmailConfirmed = true;
                user.EmailConfirmationToken = null;
                user.EmailConfirmationTokenExpiry = null;
                await _context.SaveChangesAsync();

                return new AuthResponse
                {
                    Success = true,
                    Message = "Xác thực email thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email confirmation for email: {Email}", email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình xác thực email"
                };
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null)
                {
                    return false;
                }

                user.ResetPasswordToken = GenerateRandomToken();
                user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();

                // TODO: Send password reset email
                _logger.LogInformation("Password reset token generated for user: {Email}", email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email for: {Email}", email);
                return false;
            }
        }

        public async Task<AuthResponse> ResetPasswordAsync(string email, string token, string newPassword)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Người dùng không tồn tại"
                    };
                }

                if (user.ResetPasswordToken != token || 
                    user.ResetPasswordTokenExpiry == null || 
                    user.ResetPasswordTokenExpiry < DateTime.UtcNow)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Token đặt lại mật khẩu không hợp lệ hoặc đã hết hạn"
                    };
                }

                user.PasswordHash = HashPassword(newPassword);
                user.ResetPasswordToken = null;
                user.ResetPasswordTokenExpiry = null;
                await _context.SaveChangesAsync();

                return new AuthResponse
                {
                    Success = true,
                    Message = "Đặt lại mật khẩu thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for email: {Email}", email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đặt lại mật khẩu"
                };
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "YourDefaultSecretKeyThatIsAtLeast32CharactersLong!";
            var issuer = jwtSettings["Issuer"] ?? "QAgent";
            var audience = jwtSettings["Audience"] ?? "QAgentUsers";
            var expiryInHours = int.Parse(jwtSettings["ExpiryInHours"] ?? "24");

            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim("IsEmailConfirmed", user.IsEmailConfirmed.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(expiryInHours),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool ValidateJwtToken(string token, out string userId)
        {
            userId = string.Empty;
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"] ?? "YourDefaultSecretKeyThatIsAtLeast32CharactersLong!";
                var issuer = jwtSettings["Issuer"] ?? "QAgent";
                var audience = jwtSettings["Audience"] ?? "QAgentUsers";

                var key = Encoding.ASCII.GetBytes(secretKey);
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        private string GenerateRandomToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
} 