using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.DTOs.Response.Product;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.Product
{
    public class ProductResponseDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public FlowerStatus FlowerStatus { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public long StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public List<ImageResponseDto> Images { get; set; } = new List<ImageResponseDto>();
        public List<CategoryResponseDto> Categories { get; set; } = new List<CategoryResponseDto>();
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ReviewResponseDto> Reviews { get; set; }
        public double AverageRating { get; set; }

    }
}