using System;

namespace BackEnd_FLOWER_SHOP.DTO.Response.Loyalty
{
    public class LoyaltyTransactionDto
    {
        public long Id { get; set; }
        public decimal PointsChange { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
