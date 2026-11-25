using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserContext _context;

        public AuthController(IAuthService authService, UserContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Error("Datos de inicio de sesi\u00f3n inv\u00e1lidos."));
            }

            var result = await _authService.AuthenticateAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(ApiResponse<string>.Error("Credenciales inv\u00e1lidas. Correo o contrase\u00f1a incorrectos.", 401));
            }

            return Ok(ApiResponse<AuthResponseDto>.Success(result, "Autenticaci\u00f3n exitosa"));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Error("Datos de registro incompletos o inv\u00e1lidos."));
            }

            var (success, errorMessage, response) = await _authService.RegisterAsync(registerDto);
            if (!success)
            {
                return BadRequest(ApiResponse<string>.Error(errorMessage ?? "Error al registrar usuario."));
            }

            return StatusCode(201, ApiResponse<AuthResponseDto>.Success(response!, "Usuario registrado con \u00e9xito"));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            string? userIdStr = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdStr, out long userId))
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    return Ok(ApiResponse<User>.Success(user));
                }
            }

            return Unauthorized(ApiResponse<string>.Error("Usuario no autorizado.", 401));
        }
    }
}
