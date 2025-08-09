using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.Product
{
    public class ProductSummaryDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public FlowerStatus FlowerStatus { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public decimal CurrentPrice { get; set; } // Price after applying pricing rules
        public long StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<CategoryResponseDto> Categories { get; set; } = new List<CategoryResponseDto>();
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsInStock => StockQuantity > 0;
    }
}