using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTOs.Request.Cart;
using BackEnd_FLOWER_SHOP.DTOs.Response.Cart;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class CartService : ICartService
    {
    private readonly ApplicationDbContext _context;
    private readonly IPricingService _pricingService;

        public CartService(ApplicationDbContext context, IPricingService pricingService)
        {
            _context = context;
            _pricingService = pricingService;
        }

        public async Task<CartResponseDto> GetCartByUserIdAsync(long userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ImageUploads)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Create a new cart if it doesn't exist
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                // Reload with includes
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                            .ThenInclude(p => p.ImageUploads)
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == cart.Id);
            }

            return MapToCartResponseDto(cart);
        }

        public async Task<CartResponseDto> AddToCartAsync(long userId, AddCartItemDto addToCartDto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == addToCartDto.ProductId);

            if (product == null)
                throw new ArgumentException("Product not found");

            if (!product.IsActive)
                throw new InvalidOperationException("Product is not available");

            if (product.StockQuantity < addToCartDto.Quantity)
                throw new InvalidOperationException("Insufficient stock");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingCartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == addToCartDto.ProductId);

            if (existingCartItem != null)
            {
                var newQuantity = existingCartItem.Quantity + addToCartDto.Quantity;

                if (product.StockQuantity < newQuantity)
                    throw new InvalidOperationException("Insufficient stock for requested quantity");

                existingCartItem.Quantity = newQuantity;
                existingCartItem.Price = product.BasePrice;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    Price = product.BasePrice
                };
                cart.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return await GetCartByUserIdAsync(userId);
        }

        public async Task<CartResponseDto> UpdateCartItemAsync(long userId, List<UpdateCartItemDto> updateCartItemDtos)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                throw new ArgumentException("Cart not found");

            foreach (var updateDto in updateCartItemDtos)
            {
                var cartItem = cart.CartItems
                    .FirstOrDefault(ci => ci.Id == updateDto.CartItemId);

                if (cartItem == null)
                    throw new ArgumentException($"Cart item with ID {updateDto.CartItemId} not found");

                if (updateDto.Quantity == 0)
                {
                    cart.CartItems.Remove(cartItem);
                }
                else
                {
                    if (cartItem.Product.StockQuantity < updateDto.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for product {cartItem.Product.Name}");

                    cartItem.Quantity = updateDto.Quantity;
                }
            }

            await _context.SaveChangesAsync();

            return await GetCartByUserIdAsync(userId);
        }

        public async Task<bool> RemoveFromCartAsync(long userId, long cartItemId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return false;

            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.Id == cartItemId);

            if (cartItem == null)
                return false;

            cart.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearCartAsync(long userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return false;

            cart.CartItems.Clear();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCartItemCountAsync(long userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.CartItems.Sum(ci => (int)ci.Quantity) ?? 0;
        }

        public async Task<decimal> GetCartTotalAsync(long userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.CartItems.Sum(ci => ci.Price * ci.Quantity) ?? 0;
        }

        private CartResponseDto MapToCartResponseDto(Cart cart)
        {
            var cartItems = new List<CartItemResponseDto>();
            foreach (var ci in cart.CartItems)
            {
                decimal price = ci.Price;
                if (_pricingService != null && ci.Product != null)
                {
                    // Calculate dynamic price for each product in the cart
                    price = _pricingService.CalculateDynamicPriceAsync(ci.ProductId).GetAwaiter().GetResult();
                }
                cartItems.Add(new CartItemResponseDto
                {
                    Id = ci.Id,
                    CartId = ci.CartId,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product?.Name ?? "",
                    BasePrice = ci.Price,
                    Price = price,
                    Quantity = ci.Quantity,
                    SubTotal = price * ci.Quantity,
                    ProductImage = ci.Product?.ImageUploads?.FirstOrDefault()?.ImageUrl ?? ""
                });
            }
            return new CartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                UserName = cart.User?.UserName ?? "",
                CartItems = cartItems,
                TotalAmount = cartItems.Sum(ci => ci.SubTotal),
                TotalItems = (int)cartItems.Sum(ci => ci.Quantity)
            };
        }
    }
}