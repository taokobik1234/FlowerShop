using System.ComponentModel.DataAnnotations;

namespace BackEnd_FLOWER_SHOP.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        public long AddressId { get; set; }
    }
}