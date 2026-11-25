using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.AuthenticateAsync(loginDto);
            if (result == null)
            {
                return Unauthorized(new { message = "Credenciales incorrectas" });
            }

            // Return { token = "..." } exactly as original Angular and Postman expects
            return Ok(new
            {
                token = result.token,
                userId = result.userId,
                firstName = result.firstName,
                lastName = result.lastName,
                email = result.email
            });
        }
    }
}
