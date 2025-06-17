using System.ComponentModel.DataAnnotations;

namespace BackEnd_FLOWER_SHOP.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public string NewStatus { get; set; } // e.g., "Pending", "Shipped", "Delivered"
    }
}