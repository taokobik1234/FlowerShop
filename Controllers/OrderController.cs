using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all actions in this controller
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/Orders
        // Creates a new order from the user's cart
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            var userId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Fetch the user's cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest("Cart is empty or does not exist for this user.");
            }

            // Get the address
            var address = await _context.Addresses.FindAsync(createOrderDto.AddressId);
            if (address == null || address.ApplicationUserId != user.Id)
            {
                return BadRequest("Invalid address ID or address does not belong to the user.");
            }

            var order = new Order
            {
                UserId = user.Id,
                AddressId = address.Id,
                OrderStatus = ShippingStatus.Pending, // Initial status
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
                    UserId = user.Id // Link OrderItem to the user as well
                };
                order.OrderItems.Add(orderItem);

                // Optionally, deduct stock from product
                cartItem.Product.StockQuantity -= cartItem.Quantity;
            }

            // Calculate total sum for the order (not mapped to DB, but useful for DTO)
            order.Sum = order.OrderItems.Sum(oi => (decimal)oi.Price * oi.Quantity);

            _context.Orders.Add(order);
            _context.Carts.Remove(cart); // Clear the cart after creating order

            await _context.SaveChangesAsync();

            var orderDto = new OrderDto
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

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, orderDto);
        }

        // GET: api/Orders/{id}
        // Retrieves a specific order by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(long id)
        {
            var userId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == long.Parse(userId));

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found or does not belong to the current user.");
            }

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                AddressId = order.AddressId,
                TrackingNumber = order.TrackingNumber,
                OrderStatus = order.OrderStatus.ToString(),
                Sum = order.OrderItems.Sum(oi => (decimal)oi.Price * oi.Quantity), // Calculate sum on retrieval
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
                Address = new AddressDto // Include address details
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

            return Ok(orderDto);
        }

        // GET: api/Orders/user
        // Retrieves all orders for the authenticated user
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
        {
            var userId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var orders = await _context.Orders
                .Where(o => o.UserId == long.Parse(userId))
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Address)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderDtos = orders.Select(order => new OrderDto
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

            return Ok(orderDtos);
        }

        // PUT: api/Orders/{id}/status
        // Updates the status of an order (e.g., for admin/staff)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")] // Only Admin can change order status
        public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }

            if (!Enum.TryParse(updateOrderStatusDto.NewStatus, true, out ShippingStatus newStatus))
            {
                return BadRequest("Invalid order status provided.");
            }

            order.OrderStatus = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        private bool OrderExists(long id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}