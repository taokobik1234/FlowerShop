using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.PricingRule
{
    public class DynamicPriceResponseDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DynamicPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string? AppliedRule { get; set; }
        public DateTime CalculatedAt { get; set; }
        public bool HasDiscount => DynamicPrice < BasePrice;
        public bool HasSurcharge => DynamicPrice > BasePrice;
    }
}