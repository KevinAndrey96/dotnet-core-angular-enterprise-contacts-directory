using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string firstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string lastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string documentType { get; set; } = "CC";

        [Required]
        [MaxLength(20)]
        public string documentNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string password { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
