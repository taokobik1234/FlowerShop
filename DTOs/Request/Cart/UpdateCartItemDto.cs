using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.Cart
{
    public class UpdateCartItemDto
    {
        [Required]
        public long CartItemId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be at least 0")]
        public long Quantity { get; set; }
    }
}