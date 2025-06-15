using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.Product
{
    public class ProductCreateDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public FlowerStatus FlowerStatus { get; set; }
        public string Description { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
        public decimal BasePrice { get; set; }
        public string Condition { get; set; }
        [Required]
        [Range(0, long.MaxValue, ErrorMessage = "Stock quantity must be non-negative")]
        public long StockQuantity { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

        [Required]
        public List<long> CategoryIds { get; set; }
    }
}