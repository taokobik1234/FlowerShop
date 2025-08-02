using System.ComponentModel.DataAnnotations;

namespace BackEnd_FLOWER_SHOP.DTO.Request.Loyalty
{
    public class UpdateLoyaltyPointsRequestDto
    {
        [Required]
        public decimal NewPointsValue { get; set; }
    }
}
