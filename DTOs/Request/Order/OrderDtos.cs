using System.ComponentModel.DataAnnotations;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.DTOs.Response.Address; // Added reference to your AddressDTO namespace

namespace BackEnd_FLOWER_SHOP.Dtos.Order
{
    // Assuming you have a ProductDto for displaying product information within an order item.
    // If not, it would look something like this (simplified):
    public class ProductDtoForOrderItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public FlowerStatus FlowerStatus { get; set; }
        public string Condition { get; set; }
        public ICollection<ImageUploadDto> ImageUploads { get; set; } // Assuming ImageUploadDto exists
    }

    // Assuming an ImageUploadDto exists from your Product/Image API
    public class ImageUploadDto
    {
        public string ImageUrl { get; set; }
        public string PublicId { get; set; }
    }


    /// <summary>
    /// DTO for creating a new order from an existing cart.
    /// </summary>
    public class CreateOrderRequestDto
    {
        [Required(ErrorMessage = "CartId is required.")]
        public long CartId { get; set; }

        [Required(ErrorMessage = "AddressId is required.")]
        public long AddressId { get; set; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        public PaymentMethod PaymentMethod { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing order's status and tracking number.
    /// </summary>
    public class UpdateOrderRequestDto
    {
        public ShippingStatus? OrderStatus { get; set; }
        public string? TrackingNumber { get; set; }
    }

    /// <summary>
    /// DTO for representing an individual order item in a response.
    /// </summary>
    public class OrderItemDto
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public ProductDtoForOrderItem Product { get; set; } // Simplified product info
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
