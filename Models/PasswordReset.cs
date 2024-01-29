using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Models
{
    [Index("Email", IsUnique = true)]
    public class PasswordReset
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
