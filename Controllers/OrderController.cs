using BackEnd_FLOWER_SHOP.Dtos.Response.Order;
using BackEnd_FLOWER_SHOP.Dtos.Request.Order;
using BackEnd_FLOWER_SHOP.Services.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Creates a new order for the authenticated user from their cart.
        /// </summary>
        /// <param name="createOrderDto">The order creation request containing cart and address details.</param>
        /// <returns>A 201 Created response with the new order details, or 400 Bad Request if validation fails.</returns>
        [HttpPost]
        [Authorize] // Requires authentication
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            try
            {
                var order = await _orderService.CreateOrderFromCartAsync(long.Parse(userId), createOrderDto);
                return CreatedAtAction(nameof(GetOrderById), new { orderId = order.Id }, order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error creating order: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the order." });
            }
        }

        /// <summary>
        /// Retrieves a specific order by ID for the authenticated user.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>A 200 OK response with the order details, or 404 Not Found if the order does not exist or doesn't belong to the user.</returns>
        [HttpGet("{orderId}")]
        [Authorize] // Requires authentication
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(long orderId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var order = await _orderService.GetOrderByIdForUserAsync(long.Parse(userId), orderId);

            if (order == null)
            {
                return NotFound($"Order with ID {orderId} not found or does not belong to the user.");
            }

            return Ok(order);
        }

        /// <summary>
        /// Retrieves all orders for the authenticated user.
        /// </summary>
        /// <returns>A 200 OK response with a list of the user's orders.</returns>
        [HttpGet("my-orders")]
        [Authorize] // Requires authentication
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var orders = await _orderService.GetMyOrdersAsync(long.Parse(userId));
            return Ok(orders);
        }

        /// <summary>
        /// Retrieves all orders (Admin only).
        /// </summary>
        /// <returns>A 200 OK response with a list of all orders.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")] // Requires Admin role
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // For unauthorized roles
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        /// <summary>
        /// Updates the status or tracking number of an order by ID (Admin only).
        /// </summary>
        /// <param name="orderId">The ID of the order to update.</param>
        /// <param name="updateOrderDto">The update request containing new status or tracking number.</param>
        /// <returns>A 200 OK response with the updated order details, or 400 Bad Request if validation fails/order not found.</returns>
        [HttpPut("{orderId}")]
        [Authorize(Roles = "Admin")] // Requires Admin role
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(long orderId, [FromBody] UpdateOrderRequestDto updateOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedOrder = await _orderService.UpdateOrderStatusAsync(orderId, updateOrderDto);
                return Ok(updatedOrder);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order status: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while updating the order status." });
            }
        }

        /// <summary>
        /// Deletes an order by ID (Admin only).
        /// </summary>
        /// <param name="orderId">The ID of the order to delete.</param>
        /// <returns>A 204 No Content response if successful, or 404 Not Found if the order does not exist.</returns>
        [HttpDelete("{orderId}")]
        [Authorize(Roles = "Admin")] // Requires Admin role
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrder(long orderId)
        {
            var isDeleted = await _orderService.DeleteOrderAsync(orderId);
            if (!isDeleted)
            {
                return NotFound($"Order with ID {orderId} not found.");
            }
            return NoContent(); // 204 No Content for successful deletion
        }
    }
}
