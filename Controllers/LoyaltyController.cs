using BackEnd_FLOWER_SHOP.Services.Interfaces;
using BackEnd_FLOWER_SHOP.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BackEnd_FLOWER_SHOP.Entities;
using System.Security.Claims;
using BackEnd_FLOWER_SHOP.DTO.Response.Loyalty;
using BackEnd_FLOWER_SHOP.DTO.Request.Loyalty;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
    }
}
