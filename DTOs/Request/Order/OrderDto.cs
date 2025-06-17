using System;
using System.Collections.Generic;

namespace BackEnd_FLOWER_SHOP.DTOs
{
    public class OrderDto
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public long AddressId { get; set; }
        public string TrackingNumber { get; set; }
        public string OrderStatus { get; set; } // Represent enum as string for API
        public decimal Sum { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<OrderItemDto> OrderItems { get; set; }
        public AddressDto Address { get; set; } // To include address details
    }
}