using System.ComponentModel.DataAnnotations;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.DTOs.Response.Address; // Added reference to your AddressDTO namespace
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;
namespace BackEnd_FLOWER_SHOP.Dtos.Request.Order
{
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
}
