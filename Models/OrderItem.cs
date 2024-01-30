using Microsoft.EntityFrameworkCore;

namespace ecommerce.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }

        // Navigation Properties
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
