using BackEnd_FLOWER_SHOP.DTO.Response.Loyalty;
using BackEnd_FLOWER_SHOP.Entities;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface ILoyaltyService
    {
        Task<UserLoyaltyDto> GetUserLoyaltyInfo(long userId);
        Task AddPoints(long userId, decimal amount, string description);
        Task<bool> TryRedeemPoints(long userId, decimal pointsToRedeem, string description);
    }
}
