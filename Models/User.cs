using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Models
{
    [Index("Email", IsUnique = true)]
    public class User
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Pasword { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
