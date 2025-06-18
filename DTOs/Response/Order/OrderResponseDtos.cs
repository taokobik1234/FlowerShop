using System.ComponentModel.DataAnnotations;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.DTOs.Response.Address; // Added reference to your AddressDTO namespace
using BackEnd_FLOWER_SHOP.Dtos.Request.Order;
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;

namespace BackEnd_FLOWER_SHOP.Dtos.Response.Order
{
    public class OrderItemDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public ProductResponseDto Product { get; set; } // Uses your existing ProductResponseDto
        public long Quantity { get; set; }
        public decimal Price { get; set; } // Price at the time of order
        public string Name { get; set; } // Product name at the time of order
    }

    /// <summary>
    /// DTO for representing an order in a response.
    /// </summary>
    public class OrderDto
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; } // To display user's name
        public AddressDTO Address { get; set; } // Using your provided AddressDTO
        public string TrackingNumber { get; set; }
        public ShippingStatus OrderStatus { get; set; }
        public decimal Sum { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; }
    }
}
