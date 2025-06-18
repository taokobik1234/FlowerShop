using BackEnd_FLOWER_SHOP.Dtos.Response.Order;
using BackEnd_FLOWER_SHOP.Dtos.Request.Order;
using BackEnd_FLOWER_SHOP.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Services.Order
{
    /// <summary>
    /// Interface for the Order Service, defining operations related to managing orders.
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Creates a new order from a user's cart.
        /// </summary>
        /// <param name="userId">The ID of the user placing the order.</param>
        /// <param name="createOrderDto">DTO containing cart and address information.</param>
        /// <returns>The created OrderDto.</returns>
        Task<OrderDto> CreateOrderFromCartAsync(long userId, CreateOrderRequestDto createOrderDto);

        /// <summary>
        /// Retrieves an order by its ID for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user requesting the order.</param>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>The OrderDto if found, otherwise null.</returns>
        Task<OrderDto> GetOrderByIdForUserAsync(long userId, long orderId);

        /// <summary>
        /// Retrieves all orders for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of OrderDtos.</returns>
        Task<IEnumerable<OrderDto>> GetMyOrdersAsync(long userId);

        /// <summary>
        /// Retrieves all orders (Admin access).
        /// </summary>
        /// <returns>A list of OrderDtos.</returns>
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();

        /// <summary>
        /// Updates the status and/or tracking number of an order (Admin access).
        /// </summary>
        /// <param name="orderId">The ID of the order to update.</param>
        /// <param name="updateOrderDto">DTO containing updated order information.</param>
        /// <returns>The updated OrderDto.</returns>
        Task<OrderDto> UpdateOrderStatusAsync(long orderId, UpdateOrderRequestDto updateOrderDto);

        /// <summary>
        /// Deletes an order (Admin access).
        /// </summary>
        /// <param name="orderId">The ID of the order to delete.</param>
        /// <returns>True if deletion was successful, otherwise false.</returns>
        Task<bool> DeleteOrderAsync(long orderId);
    }
}
