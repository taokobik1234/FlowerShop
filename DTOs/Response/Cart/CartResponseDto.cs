using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.Cart
{
    public class CartResponseDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public ICollection<CartItemResponseDto> CartItems { get; set; } = new List<CartItemResponseDto>();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }
}