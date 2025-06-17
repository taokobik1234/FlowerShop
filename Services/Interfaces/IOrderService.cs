using BackEnd_FLOWER_SHOP.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces // <--- Corrected Namespace
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(string userId, CreateOrderDto createOrderDto);
        Task<OrderDto> GetOrderByIdAsync(string userId, long orderId);
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(string userId);
        Task<bool> UpdateOrderStatusAsync(long orderId, string newStatus);
    }
}