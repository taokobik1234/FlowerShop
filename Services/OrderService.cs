using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.Dtos.Order;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity; // For UserManager
using BackEnd_FLOWER_SHOP.DTOs.Response.Address; // Added reference to your AddressDTO namespace

namespace BackEnd_FLOWER_SHOP.Services.Order
{
    /// <summary>
    /// Service for managing orders, including creation, retrieval, and updates.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Assuming you have product, cart, and address services for dependency injection
        // If not, you might need to adjust how product and cart data is fetched.
        // For simplicity, I'll directly interact with DbSets where necessary,
        // but in a real application, you'd likely inject IProductService, ICartService, etc.
        // private readonly IProductService _productService;
        // private readonly ICartService _cartService;
        // private readonly IAddressService _addressService;

        public OrderService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager
            // IProductService productService,
            // ICartService cartService,
            // IAddressService addressService
            )
        {
            _context = context;
            _userManager = userManager;
            // _productService = productService;
            // _cartService = cartService;
            // _addressService = addressService;
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
                        .ThenInclude(p => p.ImageUploads) // Include images for ProductDtoForOrderItem
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
                OrderStatus = ShippingStatus.Pending, // Default status
                PaymentMethod = createOrderDto.PaymentMethod,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                TrackingNumber = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 15).ToUpper(), // Simple tracking number
                OrderItems = new List<OrderItem>()
            };

            decimal totalSum = 0;

            // 4. Process Cart Items into Order Items and Update Product Stock
            foreach (var cartItem in cart.CartItems)
            {
                var product = cartItem.Product;

                if (product == null)
                {
                    // This should ideally not happen if cart items are correctly linked
                    // but as a safeguard, skip or throw an error.
                    continue;
                }

                if (product.StockQuantity < cartItem.Quantity)
                {
                    throw new ArgumentException($"Insufficient stock for product: {product.Name}. Available: {product.StockQuantity}, Requested: {cartItem.Quantity}");
                }

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = cartItem.Quantity,
                    Price = (int)cartItem.Price, // Cast to int as per OrderItem entity
                    Name = product.Name, // Store product name at time of order
                    UserId = userId // Associate order item with the user (optional, can be inferred from order)
                };
                order.OrderItems.Add(orderItem);

                // Decrement product stock
                product.StockQuantity -= cartItem.Quantity;
                _context.Products.Update(product);

                totalSum += cartItem.Price * cartItem.Quantity;
            }

            order.Sum = totalSum; // Calculate total sum for the order

            // 5. Add Order to Database
            _context.Orders.Add(order);

            // 6. Clear the user's cart after order creation
            _context.CartItem.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart); // Or just clear cart.CartItems if you want to keep the cart entity

            await _context.SaveChangesAsync();

            // 7. Map to DTO and return
            var user = await _userManager.FindByIdAsync(userId.ToString());

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = user?.UserName, // Or user.FirstName + " " + user.LastName
                Address = new AddressDTO // Using your provided AddressDTO
                {
                    Id = address.Id,
                    FirstName = address.FirstName,
                    LastName = address.LastName,
                    StreetAddress = address.StreetAddress,
                    City = address.City,
                    Country = address.Country,
                    ZipCode = address.ZipCode,
                    ApplicationUserId = address.ApplicationUserId // Added this property from your DTO
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
                    Product = new ProductDtoForOrderItem // Map simplified product details
                    {
                        Id = oi.Product.Id,
                        Name = oi.Product.Name,
                        BasePrice = oi.Product.BasePrice,
                        Description = oi.Product.Description,
                        FlowerStatus = oi.Product.flowerstatus,
                        Condition = oi.Product.Condition,
                        ImageUploads = oi.Product.ImageUploads.Select(iu => new ImageUploadDto
                        {
                            ImageUrl = iu.ImageUrl,
                            PublicId = iu.PublicId
                        }).ToList()
                    },
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
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                UserName = order.User?.UserName, // Use null conditional operator
                Address = order.Address != null ? new AddressDTO // Using your provided AddressDTO
                {
                    Id = order.Address.Id,
                    FirstName = order.Address.FirstName,
                    LastName = order.Address.LastName,
                    StreetAddress = order.Address.StreetAddress,
                    City = order.Address.City,
                    Country = order.Address.Country,
                    ZipCode = order.Address.ZipCode,
                    ApplicationUserId = order.Address.ApplicationUserId // Added this property from your DTO
                } : null,
                TrackingNumber = order.TrackingNumber,
                OrderStatus = order.OrderStatus,
                Sum = order.Sum,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                PaymentMethod = order.PaymentMethod,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    Product = oi.Product != null ? new ProductDtoForOrderItem // Map simplified product details
                    {
                        Id = oi.Product.Id,
                        Name = oi.Product.Name,
                        BasePrice = oi.Product.BasePrice,
                        Description = oi.Product.Description,
                        FlowerStatus = oi.Product.flowerstatus,
                        Condition = oi.Product.Condition,
                        ImageUploads = oi.Product.ImageUploads?.Select(iu => new ImageUploadDto
                        {
                            ImageUrl = iu.ImageUrl,
                            PublicId = iu.PublicId
                        }).ToList() ?? new List<ImageUploadDto>()
                    } : null,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                    Name = oi.Name
                }).ToList() ?? new List<OrderItemDto>()
            };
        }
    }
}
