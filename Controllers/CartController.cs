using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request.Cart;
using BackEnd_FLOWER_SHOP.DTOs.Response.Cart;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [Route("api/Cart")]
    [Authorize(Roles = "User")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IUserService _userService;

        public CartController(ICartService cartService, IUserService userService)
        {
            _cartService = cartService;
            _userService = userService;
        }

        /// <summary>
        /// Get current user's cart
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            try
            {
                var currentUserId = long.Parse(_userService.GetCurrentUserId());
                var cart = await _cartService.GetCartByUserIdAsync(currentUserId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost("add")]
        public async Task<ActionResult<CartResponseDto>> AddToCart([FromBody] AddCartItemDto addToCartDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = long.Parse(_userService.GetCurrentUserId());
                var cart = await _cartService.AddToCartAsync(currentUserId, addToCartDto);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding item to cart" });
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("update")]
        public async Task<ActionResult<CartResponseDto>> UpdateCartItems([FromBody] List<UpdateCartItemDto> updateCartItemDtos)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var currentUserId = long.Parse(_userService.GetCurrentUserId());
                var cart = await _cartService.UpdateCartItemAsync(currentUserId, updateCartItemDtos);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating cart items" });
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("remove/{cartItemId}")]
        public async Task<ActionResult> RemoveFromCart(long cartItemId)
        {
            try
            {
                var currentUserId = long.Parse(_userService.GetCurrentUserId());
                var result = await _cartService.RemoveFromCartAsync(currentUserId, cartItemId);

                if (!result)
                    return NotFound(new { message = "Cart item not found" });

                return Ok(new { message = "Item removed from cart successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing item from cart" });
            }
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        [HttpDelete("clear")]
        public async Task<ActionResult> ClearCart()
        {
            try
            {
                var currentUserId = long.Parse(_userService.GetCurrentUserId());
                var result = await _cartService.ClearCartAsync(currentUserId);

                if (!result)
                    return NotFound(new { message = "Cart not found" });

                return Ok(new { message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while clearing cart" });
            }
        }

        /// <summary>
        /// Get cart item count
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetCartItemCount()
        {
            try
            {
                var currentUserId = long.Parse(_userService.GetCurrentUserId());
                var count = await _cartService.GetCartItemCountAsync(currentUserId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting cart count" });
            }
        }

        /// <summary>
        /// Get cart total amount
        /// </summary>
        [HttpGet("total")]
        public async Task<ActionResult<decimal>> GetCartTotal()
        {
            try
            {
                var currentUserId = long.Parse(_userService.GetCurrentUserId());
                var total = await _cartService.GetCartTotalAsync(currentUserId);
                return Ok(total);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting cart total" });
            }
        }

        /// <summary>
        /// Get cart by user ID (Admin only)
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CartResponseDto>> GetCartByUserId(long userId)
        {
            try
            {
                var cart = await _cartService.GetCartByUserIdAsync(userId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}