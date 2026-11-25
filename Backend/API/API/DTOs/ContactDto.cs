using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ContactDto
    {
        public long id { get; set; }
        public long idUser { get; set; }
        public long? idContact { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string documentType { get; set; } = "CC";
        public string documentNumber { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string relationship { get; set; } = "Amigo";
        public bool isRegisteredUser { get; set; }
    }

    public class CreateContactDto
    {
        [Required]
        public long idUser { get; set; }

        public long? idContact { get; set; }

        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? documentType { get; set; }
        public string? documentNumber { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }

        [Required(ErrorMessage = "El tipo de relaci\u00f3n es obligatorio (Amigo, Familiar, Colega, Otro)")]
        public string relationship { get; set; } = "Amigo";
    }

    public class UpdateContactDto
    {
        [Required]
        public string relationship { get; set; } = "Amigo";
        public string? phone { get; set; }
        public string? email { get; set; }
    }
}
