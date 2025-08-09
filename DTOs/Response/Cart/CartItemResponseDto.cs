using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.Cart
{
    public class CartItemResponseDto
    {
        public long Id { get; set; }
        public long CartId { get; set; }
        public long ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal BasePrice { get; set; }
        public long Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
    }
}