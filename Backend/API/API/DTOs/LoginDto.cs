using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El correo de usuario es obligatorio")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrase\u00f1a es obligatoria")]
        public string Pass { get; set; } = string.Empty;
    }
}
