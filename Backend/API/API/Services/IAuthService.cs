using System.Threading.Tasks;
using API.DTOs;
using API.Models;

namespace API.Services
{
    public interface IAuthService
    {
        string GenerateJwtToken(User user);
        Task<AuthResponseDto?> AuthenticateAsync(LoginDto loginDto);
        Task<(bool Success, string? ErrorMessage, AuthResponseDto? Response)> RegisterAsync(RegisterDto registerDto);
    }
}
