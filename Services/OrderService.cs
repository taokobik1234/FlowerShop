using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTOs;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.Services.Interfaces; // <--- ADD THIS USING DIRECTIVE
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Services // <--- Corrected Namespace
{
    public class OrderService : IOrderService // <--- ENSURE it implements IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto createOrderDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new ArgumentException("Cart is empty or does not exist for this user.");
            }

            var address = await _context.Addresses.FindAsync(createOrderDto.AddressId);
            if (address == null || address.ApplicationUserId != user.Id)
            {
                throw new ArgumentException("Invalid address ID or address does not belong to the user.");
            }

            var order = new Order
            {
                UserId = user.Id,
                AddressId = address.Id,
                OrderStatus = ShippingStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Price = (int)cartItem.Price, // Assuming Price in OrderItem is int, adjust if decimal
                    Name = cartItem.Product.Name,
                    UserId = user.Id
                };
                order.OrderItems.Add(orderItem);

                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            order.Sum = order.OrderItems.Sum(oi => (decimal)oi.Price * oi.Quantity);

            _context.Orders.Add(order);
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TrackingNumber = order.TrackingNumber,
                OrderStatus = order.OrderStatus.ToString(),
                Sum = order.Sum,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Name = oi.Name
                }).ToList()
            };
        }

        public async Task<OrderDto> GetOrderByIdAsync(string userId, long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == long.Parse(userId));

            if (order == null)
            {
                return null;
            }

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TrackingNumber = order.TrackingNumber,
                OrderStatus = order.OrderStatus.ToString(),
                Sum = order.OrderItems.Sum(oi => (decimal)oi.Price * oi.Quantity),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Name = oi.Name
                }).ToList(),
                Address = new AddressDto
                {
                    Id = order.Address.Id,
                    FirstName = order.Address.FirstName,
                    LastName = order.Address.LastName,
                    StreetAddress = order.Address.StreetAddress,
                    City = order.Address.City,
                    Country = order.Address.Country,
                    ZipCode = order.Address.ZipCode
                }
            };
        }

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == long.Parse(userId))
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(order => new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TrackingNumber = order.TrackingNumber,
                OrderStatus = order.OrderStatus.ToString(),
                Sum = order.OrderItems.Sum(oi => (decimal)oi.Price * oi.Quantity),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Name = oi.Name
                }).ToList(),
                Address = new AddressDto
                {
                    Id = order.Address.Id,
                    FirstName = order.Address.FirstName,
                    LastName = order.Address.LastName,
                    StreetAddress = order.Address.StreetAddress,
                    City = order.Address.City,
                    Country = order.Address.Country,
                    ZipCode = order.Address.ZipCode
                }
            }).ToList();
        }

        public async Task<bool> UpdateOrderStatusAsync(long orderId, string newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return false;
            }

            if (!Enum.TryParse(newStatus, true, out ShippingStatus statusEnum))
            {
                throw new ArgumentException("Invalid order status provided.");
            }

            order.OrderStatus = statusEnum;
            order.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Orders.Any(e => e.Id == orderId))
                {
                    return false;
                }
                throw;
            }
        }
    }
}