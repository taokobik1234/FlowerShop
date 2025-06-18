using System.ComponentModel.DataAnnotations;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.DTOs.Response.Address; // Added reference to your AddressDTO namespace
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;
namespace BackEnd_FLOWER_SHOP.Dtos.Request.Order
{

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
}
