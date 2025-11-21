using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new { message = "Email, senha e nome de usuário são obrigatórios" });
            }
            
            var user = await _authService.RegisterAsync(request.Email, request.Password, request.Username);
            
            if (user == null)
            {
                return BadRequest(new { message = "Email ou nome de usuário já cadastrado" });
            }
            
            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                username = user.Username,
                vbucks = user.Vbucks,
                message = "Usuário cadastrado com sucesso! Você recebeu 10.000 v-bucks."
            });
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email e senha são obrigatórios" });
            }
            
            var user = await _authService.LoginAsync(request.Email, request.Password);
            
            if (user == null)
            {
                return Unauthorized(new { message = "Email ou senha incorretos" });
            }
            
            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                username = user.Username,
                vbucks = user.Vbucks
            });
        }
        
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser([FromHeader(Name = "X-User-Id")] int? userId)
        {
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Usuário não autenticado" });
            }
            
            var user = await _authService.GetUserByIdAsync(userId.Value);
            
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }
            
            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                username = user.Username,
                vbucks = user.Vbucks
            });
        }
    }
    
    public class RegisterRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Username { get; set; } = "";
    }
    
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}

