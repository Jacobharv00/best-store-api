using System.ComponentModel.DataAnnotations;

namespace ecommerce.Models
{
    public class UserDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required, MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        [Required, MinLength(8), MaxLength(100)]
        public string Pasword { get; set; } = string.Empty;
    }
}
