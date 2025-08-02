namespace BackEnd_FLOWER_SHOP.DTO.Response.Loyalty
{
    public class UserSummaryLoyaltyDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public decimal LoyaltyPoints { get; set; }
    }
}