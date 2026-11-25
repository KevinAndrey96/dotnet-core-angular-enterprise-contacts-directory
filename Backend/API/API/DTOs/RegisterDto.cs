using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100)]
        public string firstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [MaxLength(100)]
        public string lastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [MaxLength(10)]
        public string documentType { get; set; } = "CC";

        [Required(ErrorMessage = "El n\u00famero de documento es obligatorio")]
        [MaxLength(20)]
        public string documentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "El n\u00famero celular es obligatorio")]
        [MaxLength(20)]
        public string phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electr\u00f3nico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato de correo electr\u00f3nico no es v\u00e1lido")]
        [MaxLength(150)]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrase\u00f1a es obligatoria")]
        [MinLength(6, ErrorMessage = "La contrase\u00f1a debe contener al menos 6 caracteres")]
        public string password { get; set; } = string.Empty;
    }
}
