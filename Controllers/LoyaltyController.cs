using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using BackEnd_FLOWER_SHOP.DTO.Request.Loyalty;
using BackEnd_FLOWER_SHOP.DTO.Response.Loyalty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.Enums;
using System;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Existing user-facing endpoints
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoyaltyController(ILoyaltyService loyaltyService, UserManager<ApplicationUser> userManager)
        {
            _loyaltyService = loyaltyService;
            _userManager = userManager;
        }

        private async Task<long> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user.Id;
        }

        /// <summary>
        /// Gets the current user's loyalty points and transaction history.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserLoyaltyDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserLoyaltyInfo()
        {
            var userId = await GetCurrentUserId();
            var loyaltyInfo = await _loyaltyService.GetUserLoyaltyInfo(userId);

            if (loyaltyInfo == null)
            {
                return NotFound("User not found.");
            }

            return Ok(loyaltyInfo);
        }

        /// <summary>
        /// Redeems loyalty points for a reward.
        /// </summary>
        [HttpPost("redeem")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RedeemPoints([FromBody] RedeemRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = await GetCurrentUserId();
            var redemptionSuccess = await _loyaltyService.TryRedeemPoints(userId, request.PointsToRedeem, "Points redeemed for a reward.");

            if (!redemptionSuccess)
            {
                return BadRequest("Insufficient loyalty points to redeem.");
            }
            
            return Ok(new { message = $"Successfully redeemed {request.PointsToRedeem} points." });
        }

        /// <summary>
        /// Retrieves the loyalty points and basic info for all users.
        /// Admin access only.
        /// </summary>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(AllUsersLoyaltyDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllUserLoyaltyInfo()
        {
            var users = await _loyaltyService.GetAllUsersWithLoyaltyInfo();
            return Ok(new AllUsersLoyaltyDto { Users = users.ToList() });
        }

        /// <summary>
        /// Updates the loyalty points for a specific user.
        /// Admin access only.
        /// </summary>
        [HttpPut("update/{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserSummaryLoyaltyDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> UpdateUserLoyaltyPoints(long userId, [FromBody] UpdateLoyaltyPointsRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedUserDto = await _loyaltyService.UpdateUserPoints(userId, dto.NewPointsValue);
                if (updatedUserDto == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }
                return Ok(updatedUserDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
