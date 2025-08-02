using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTO.Response.Loyalty;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public LoyaltyService(ApplicationDbContext context, UserManager<ApplicationUser> userManager) // New: Add UserManager to constructor
        {
            _context = context;
            _userManager = userManager;
        }
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
        /// <summary>
        /// Retrieves a summary of loyalty points for all users.
        /// </summary>
        public async Task<IEnumerable<UserSummaryLoyaltyDto>> GetAllUsersWithLoyaltyInfo()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new UserSummaryLoyaltyDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    LoyaltyPoints = u.LoyaltyPoints
                })
                .ToListAsync();

            return users;
        }

        /// <summary>
        /// Updates a user's loyalty points and records the change as a transaction.
        /// </summary>
        public async Task<UserSummaryLoyaltyDto> UpdateUserPoints(long userId, decimal newPointsValue)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return null;
            }
            
            var change = newPointsValue - user.LoyaltyPoints;
            string description = $"Admin update. Points changed from {user.LoyaltyPoints} to {newPointsValue}.";

            user.LoyaltyPoints = newPointsValue;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new Exception($"Failed to update user loyalty points: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }
            
            // Create a loyalty transaction record
            var transaction = new LoyaltyTransaction
            {
                UserId = user.Id,
                PointsChange = change,
                Description = description,
                CreatedAt = System.DateTime.UtcNow
            };
            await _context.LoyaltyTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return new UserSummaryLoyaltyDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                LoyaltyPoints = user.LoyaltyPoints
            };
        }
    }
}