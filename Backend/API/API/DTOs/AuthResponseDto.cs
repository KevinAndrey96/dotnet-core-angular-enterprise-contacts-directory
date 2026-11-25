using API.Models;

namespace API.DTOs
{
    public class AuthResponseDto
    {
        public string token { get; set; } = string.Empty;
        public long userId { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string? registeredByMessage { get; set; }
    }
}
