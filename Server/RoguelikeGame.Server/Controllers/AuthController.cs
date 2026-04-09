using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RoguelikeGame.Server.Services;

namespace RoguelikeGame.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var (success, token, message) = await _authService.RegisterAsync(
                    request.Username,
                    request.Password,
                    request.Email
                );

                if (success)
                {
                    _logger.LogInformation("用户注册成功: {Username}", request.Username);
                    return Ok(new { success = true, token, message });
                }
                else
                {
                    return BadRequest(new { success = false, message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "注册失败");
                return StatusCode(500, new { success = false, message = "服务器内部错误" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var (success, token, userId, message) = await _authService.LoginAsync(
                    request.Username,
                    request.Password
                );

                if (success)
                {
                    _logger.LogInformation("用户登录成功: {Username}", request.Username);
                    return Ok(new { success = true, token, userId, message });
                }
                else
                {
                    return Unauthorized(new { success = false, message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录失败");
                return StatusCode(500, new { success = false, message = "服务器内部错误" });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Level,
                user.Experience,
                user.TotalGamesPlayed,
                user.GamesWon
            });
        }
    }

    public class RegisterRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Username { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = "";

        [EmailAddress]
        public string? Email { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}
