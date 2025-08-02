using System.Collections.Generic;

namespace BackEnd_FLOWER_SHOP.DTO.Response.Loyalty
{
    public class UserLoyaltyDto
    {
        public decimal CurrentPoints { get; set; }
        public ICollection<LoyaltyTransactionDto> Transactions { get; set; }
    }
}