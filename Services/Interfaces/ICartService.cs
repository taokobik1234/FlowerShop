using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request.Cart;
using BackEnd_FLOWER_SHOP.DTOs.Response.Cart;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartByUserIdAsync(long userId);
        Task<CartResponseDto> AddToCartAsync(long userId, AddCartItemDto addToCartDto);
        Task<CartResponseDto> UpdateCartItemAsync(long userId, UpdateCartItemDto updateCartItemDto);
        Task<bool> RemoveFromCartAsync(long userId, long cartItemId);
        Task<bool> ClearCartAsync(long userId);
        Task<int> GetCartItemCountAsync(long userId);
        Task<decimal> GetCartTotalAsync(long userId);
    }
}