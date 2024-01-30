using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        [Precision(18, 2)]
        public decimal ShippingFee { get; set; }

        [MaxLength(100)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [MaxLength(30)]
        public string PaymentMethod { get; set; } = string.Empty;

        [MaxLength(30)]
        public string PaymentStatus { get; set; } = string.Empty;

        [MaxLength(30)]
        public string OrderStatus { get; set; } = string.Empty;

        // Navigation Properties
        public User User { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
