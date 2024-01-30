using ecommerce.Models;
using ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public OrdersController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [Authorize]
        [HttpPost]
        public IActionResult CreateOrder(OrderDto orderDto)
        {
            if (!OrderHelper.PaymentMethods.ContainsKey(orderDto.PaymentMethod))
            {
                ModelState.AddModelError("Payment Method", "Please select a valid payment method");
                return BadRequest(ModelState);
            }

            int userId = JwtReader.GetUserId(User);
            var user = context.Users.Find(userId);
            if (user is null)
            {
                ModelState.AddModelError("Order", "Unable to create the order");
                return BadRequest(ModelState);
            }

            var productDictionary = OrderHelper.GetProductDictionary(orderDto.ProductIdentifiers);

            Order order = new Order();
            order.UserId = userId;
            order.CreatedAt = DateTime.Now;
            order.ShippingFee = OrderHelper.ShippingFee;
            order.DeliveryAddress = orderDto.DeliveryAddress;
            order.PaymentMethod = orderDto.PaymentMethod;
            order.PaymentStatus = OrderHelper.PaymentStatuses[0]; // Pending
            order.OrderStatus = OrderHelper.OrderStatuses[0]; // Created

            foreach (var pair in productDictionary)
            {
                int productId = pair.Key;
                var product = context.Products.Find(productId);
                if (product is null)
                {
                    ModelState.AddModelError(
                        "Product",
                        "Product with id " + productId + " is not available"
                    );
                    return BadRequest(ModelState);
                }

                var orderItem = new OrderItem();
                orderItem.ProductId = productId;
                orderItem.Quantity = pair.Value;
                orderItem.UnitPrice = product.Price;

                order.OrderItems.Add(orderItem);
            }

            if (order.OrderItems.Count < 1)
            {
                ModelState.AddModelError("Order", "Unable to create the order");
                return BadRequest(ModelState);
            }

            context.Orders.Add(order);
            context.SaveChanges();

            return Ok(order);
        }
    }
}
