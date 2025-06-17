


using BackEnd_FLOWER_SHOP.Data; // Still needed for ApplicationUser, though could be refactored
using BackEnd_FLOWER_SHOP.DTOs;
using BackEnd_FLOWER_SHOP.Entities; // Still needed for ApplicationUser, though could be refactored
using BackEnd_FLOWER_SHOP.Enums; // Potentially still needed for validation or DTO conversion if not fully moved to service
using BackEnd_FLOWER_SHOP.Services.Interfaces; // New: Reference the service interface
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System; // For ArgumentException

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all actions in this controller
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager; // Still needed to get current user ID

        public OrdersController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            try
            {
                var orderDto = await _orderService.CreateOrderAsync(userId, createOrderDto);
                if (orderDto == null)
                {
                    return NotFound("User not found (internal error, should be caught by auth).");
                }
                return CreatedAtAction(nameof(GetOrderById), new { id = orderDto.Id }, orderDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(long id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var orderDto = await _orderService.GetOrderByIdAsync(userId, id);
            if (orderDto == null)
            {
                return NotFound($"Order with ID {id} not found or does not belong to the current user.");
            }

            return Ok(orderDto);
        }

        // GET: api/Orders/user
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var orderDtos = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orderDtos);
        }

        // PUT: api/Orders/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(long id, [FromBody] UpdateOrderStatusDto updateOrderStatusDto)
        {
            try
            {
                var success = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDto.NewStatus);
                if (!success)
                {
                    return NotFound($"Order with ID {id} not found.");
                }
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateConcurrencyException)
            {
                // This catch block is important if you want to handle concurrency specifically
                // If the service indicates a concurrency issue but the order still exists
                // then you might want a different response or retry logic.
                // For now, rethrowing from service is handled as a generic 500 error,
                // but you can refine this.
                return StatusCode(500, "A concurrency error occurred while updating the order status.");
            }
        }
    }
}