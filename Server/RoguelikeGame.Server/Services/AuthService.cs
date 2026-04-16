using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoguelikeGame.Server.Data;
using RoguelikeGame.Server.Models;

namespace RoguelikeGame.Server.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Token, string Message)> RegisterAsync(string username, string password, string? email = null);
        Task<(bool Success, string Token, string UserId, string Message)> LoginAsync(string username, string password);
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> ValidateTokenAsync(string token);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Token, string Message)> RegisterAsync(string username, string password, string? email = null)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser != null)
            {
                return (false, "", "用户名已存在");
            }

            if (email != null)
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (existingEmail != null)
                {
                    return (false, "", "邮箱已被注册");
                }
            }

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return (true, token, "注册成功");
        }

        public async Task<(bool Success, string Token, string UserId, string Message)> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return (false, "", "", "用户名或密码错误");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return (false, "", "", "用户名或密码错误");
            }

            user.LastLoginAt = DateTime.UtcNow;
            user.IsOnline = true;

            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return (true, token, user.Id, "登录成功");
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default_key");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default_key");
            var expireDays = int.Parse(_configuration["Jwt:ExpireDays"] ?? "7");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(expireDays),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
