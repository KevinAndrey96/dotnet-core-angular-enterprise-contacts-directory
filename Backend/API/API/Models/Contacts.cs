using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("Contacts")]
    public class Contacts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public long IdUser { get; set; }

        public long? IdContact { get; set; }

        [MaxLength(100)]
        public string firstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string lastName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string documentType { get; set; } = "CC";

        [MaxLength(20)]
        public string documentNumber { get; set; } = string.Empty;

        [MaxLength(20)]
        public string phone { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(150)]
        public string email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string relationship { get; set; } = "Amigo";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
