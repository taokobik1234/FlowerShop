using System.ComponentModel.DataAnnotations;

namespace BackEnd_FLOWER_SHOP.DTO.Request.Loyalty
{
    public class RedeemRequestDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Points to redeem must be a positive number.")]
        public decimal PointsToRedeem { get; set; }
    }
}
