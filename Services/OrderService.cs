using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.Dtos.Response.Order;
using BackEnd_FLOWER_SHOP.Dtos.Request.Order;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using BackEnd_FLOWER_SHOP.DTOs.Response.Address;
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;
using BackEnd_FLOWER_SHOP.Services.Interfaces; // Added for ILoyaltyService

namespace BackEnd_FLOWER_SHOP.Services.Order
{
    /// <summary>
    /// Service for managing orders, including creation, retrieval, and updates.
    /// </summary>
    public class OrderService : IOrderService
    {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoyaltyService _loyaltyService; // New: Inject loyalty service
    private readonly IPricingService _pricingService;

        public OrderService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILoyaltyService loyaltyService,
            IPricingService pricingService
            )
        {
            _context = context;
            _userManager = userManager;
            _loyaltyService = loyaltyService;
            _pricingService = pricingService;
        }

        /// <summary>
        /// Creates a new order from a user's cart.
        /// This involves fetching cart items, validating products and stock,
        /// creating order items, and clearing the cart.
        /// </summary>
        /// <param name="userId">The ID of the user placing the order.</param>
        /// <param name="createOrderDto">DTO containing cart and address information.</param>
        /// <returns>The created OrderDto.</returns>
        /// <exception cref="ArgumentException">Thrown if the cart or address is not found, or if product stock is insufficient.</exception>
        public async Task<OrderDto> CreateOrderFromCartAsync(long userId, CreateOrderRequestDto createOrderDto)
        {
            // 1. Fetch Cart and Cart Items
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ImageUploads)
                .FirstOrDefaultAsync(c => c.Id == createOrderDto.CartId && c.UserId == userId);

            if (cart == null)
            {
                throw new ArgumentException("Cart not found or does not belong to the user.");
            }

            if (!cart.CartItems.Any())
            {
                throw new ArgumentException("Cart is empty. Cannot create an order.");
            }

            // 2. Fetch Shipping Address
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == createOrderDto.AddressId && a.ApplicationUserId == userId);

            if (address == null)
            {
                throw new ArgumentException("Shipping address not found or does not belong to the user.");
            }

            // 3. Initialize new Order
            var order = new Entities.Order
            {
                UserId = userId,
                AddressId = createOrderDto.AddressId,
                OrderStatus = ShippingStatus.Pending,
                PaymentMethod = createOrderDto.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TrackingNumber = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 15).ToUpper(),
                OrderItems = new List<OrderItem>()
            };

            decimal totalSum = 0;

            // 4. Process Cart Items into Order Items and Update Product Stock
            foreach (var cartItem in cart.CartItems)
            {
                var product = cartItem.Product;
                if (product == null)
                {
                    continue;
                }
                if (product.StockQuantity < cartItem.Quantity)
                {
                    throw new ArgumentException($"Insufficient stock for product: {product.Name}. Available: {product.StockQuantity}, Requested: {cartItem.Quantity}");
                }
                // Get the correct dynamic price
                decimal dynamicPrice = await _pricingService.CalculateDynamicPriceAsync(product.Id, DateTime.UtcNow);
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = cartItem.Quantity,
                    Price = dynamicPrice,
                    Name = product.Name,
                    UserId = userId
                };
                order.OrderItems.Add(orderItem);
                // Decrement product stock
                product.StockQuantity -= cartItem.Quantity;
                _context.Products.Update(product);
                totalSum += dynamicPrice * cartItem.Quantity;
            }

            order.Sum = totalSum;

            // 5. Add Order to Database
            _context.Orders.Add(order);

            // 6. Clear the user's cart after order creation
            _context.CartItem.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart);

            await _context.SaveChangesAsync();

            // 7. Award loyalty points after successful order creation
            // Example: 1 point for every $10 spent
            var pointsEarned = Math.Floor(order.Sum / 10);
            if (pointsEarned > 0)
            {
                await _loyaltyService.AddPoints(userId, pointsEarned, $"Points earned from order #{order.Id}");
            }

            // 8. Map to DTO and return
            var user = await _userManager.FindByIdAsync(userId.ToString());

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = user?.UserName,
                Address = new AddressDTO
                {
                    Id = address.Id,
                    FullName = address.FullName,
                    PhoneNumber = address.PhoneNumber,
                    StreetAddress = address.StreetAddress,
                    City = address.City,
                    ApplicationUserId = address.ApplicationUserId
                },
                TrackingNumber = order.TrackingNumber,
                OrderStatus = order.OrderStatus,
                Sum = order.Sum,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                PaymentMethod = order.PaymentMethod,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Product = oi.Product != null ? new ProductResponseDto
                    {
                        Id = oi.Product.Id,
                        Name = oi.Product.Name,
                        BasePrice = oi.Product.BasePrice,
                        Description = oi.Product.Description,
                        FlowerStatus = oi.Product.flowerstatus,
                        Images = oi.Product.ImageUploads?.Select(iu => new ImageResponseDto
                        {
                            Id = iu.Id,
                            ImageUrl = iu.ImageUrl,
                            PublicId = iu.PublicId
                        }).ToList() ?? new List<ImageResponseDto>()
                    } : null,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Name = oi.Name
                }).ToList()
            };
        }

        /// <summary>
        /// Retrieves an order by its ID for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user requesting the order.</param>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>The OrderDto if found, otherwise null.</returns>
        public async Task<OrderDto> GetOrderByIdForUserAsync(long userId, long orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ImageUploads)
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return null;
            }

            return MapOrderToDto(order);
        }

        /// <summary>
        /// Retrieves all orders for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of OrderDtos.</returns>
        public async Task<IEnumerable<OrderDto>> GetMyOrdersAsync(long userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ImageUploads)
                .Include(o => o.Address)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapOrderToDto);
        }

        /// <summary>
        /// Retrieves all orders (Admin access).
        /// </summary>
        /// <returns>A list of OrderDtos.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ImageUploads)
                .Include(o => o.Address)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapOrderToDto);
        }

        /// <summary>
        /// Updates the status and/or tracking number of an order (Admin access).
        /// </summary>
        /// <param name="orderId">The ID of the order to update.</param>
        /// <param name="updateOrderDto">DTO containing updated order information.</param>
        /// <returns>The updated OrderDto.</returns>
        /// <exception cref="ArgumentException">Thrown if the order is not found.</exception>
        public async Task<OrderDto> UpdateOrderStatusAsync(long orderId, UpdateOrderRequestDto updateOrderDto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ImageUploads)
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new ArgumentException($"Order with ID {orderId} not found.");
            }

            if (updateOrderDto.OrderStatus.HasValue)
            {
                order.OrderStatus = updateOrderDto.OrderStatus.Value;
            }

            if (!string.IsNullOrEmpty(updateOrderDto.TrackingNumber))
            {
                order.TrackingNumber = updateOrderDto.TrackingNumber;
            }

            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapOrderToDto(order);
        }

        /// <summary>
        /// Deletes an order (Admin access).
        /// </summary>
        /// <param name="orderId">The ID of the order to delete.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        public async Task<bool> DeleteOrderAsync(long orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Helper method to map an Order entity to an OrderDto.
        /// </summary>
        /// <param name="order">The Order entity to map.</param>
        /// <returns>The mapped OrderDto.</returns>
        private OrderDto MapOrderToDto(Entities.Order order)
        {
            var calculatedSum = order.OrderItems?.Sum(oi => (decimal)oi.Price * oi.Quantity) ?? 0;

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = order.User?.UserName,
                Address = order.Address != null ? new AddressDTO
                {
                    Id = order.Address.Id,
                    FullName = order.Address.FullName,
                    PhoneNumber = order.Address.PhoneNumber,
                    StreetAddress = order.Address.StreetAddress,
                    City = order.Address.City,
                    ApplicationUserId = order.Address.ApplicationUserId
                } : null,
                TrackingNumber = order.TrackingNumber,
                OrderStatus = order.OrderStatus,
                Sum = calculatedSum, // Use the dynamically calculated sum here
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                PaymentMethod = order.PaymentMethod,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Product = oi.Product != null ? new ProductResponseDto
                    {
                        Id = oi.Product.Id,
                        Name = oi.Product.Name,
                        BasePrice = oi.Product.BasePrice,
                        Description = oi.Product.Description,
                        FlowerStatus = oi.Product.flowerstatus,
                        Images = oi.Product.ImageUploads?.Select(iu => new ImageResponseDto
                        {
                            Id = iu.Id,
                            ImageUrl = iu.ImageUrl,
                            PublicId = iu.PublicId
                        }).ToList() ?? new List<ImageResponseDto>()
                    } : null,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Name = oi.Name
                }).ToList() ?? new List<OrderItemDto>()
            };
        }
    }
}
