using System.ComponentModel.DataAnnotations;

namespace ecommerce.Models
{
    public class OrderDto
    {
        [Required]
        public string ProductIdentifiers { get; set; } = string.Empty;

        [Required, MinLength(30), MaxLength(100)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
