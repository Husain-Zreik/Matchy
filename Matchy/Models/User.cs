using System;
using System.ComponentModel.DataAnnotations;

namespace Matchy.Models
{
    public class User
    {
        public int Id { get; set; }  // Primary Key

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
        public int Age { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
