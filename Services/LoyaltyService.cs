using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTO.Response.Loyalty;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly ApplicationDbContext _context;

        public LoyaltyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserLoyaltyDto> GetUserLoyaltyInfo(long userId)
        {
            var user = await _context.Users
                                     .Include(u => u.Orders) // Assuming you have orders linked
                                     .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            var transactions = await _context.LoyaltyTransactions
                                             .Where(t => t.UserId == userId)
                                             .OrderByDescending(t => t.CreatedAt)
                                             .Select(t => new LoyaltyTransactionDto
                                             {
                                                 Id = t.Id,
                                                 PointsChange = t.PointsChange,
                                                 Description = t.Description,
                                                 CreatedAt = t.CreatedAt
                                             })
                                             .ToListAsync();

            return new UserLoyaltyDto
            {
                CurrentPoints = user.LoyaltyPoints,
                Transactions = transactions
            };
        }

        public async Task AddPoints(long userId, decimal amount, string description)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                user.LoyaltyPoints += amount;
                await _context.LoyaltyTransactions.AddAsync(new LoyaltyTransaction
                {
                    UserId = userId,
                    PointsChange = amount,
                    Description = description
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> TryRedeemPoints(long userId, decimal pointsToRedeem, string description)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.LoyaltyPoints < pointsToRedeem)
            {
                return false;
            }

            user.LoyaltyPoints -= pointsToRedeem;
            await _context.LoyaltyTransactions.AddAsync(new LoyaltyTransaction
            {
                UserId = userId,
                PointsChange = -pointsToRedeem,
                Description = description
            });
            await _context.SaveChangesAsync();

            return true;
        }
    }
}