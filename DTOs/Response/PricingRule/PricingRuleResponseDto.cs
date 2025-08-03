using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.PricingRule
{
    public class PricingRuleResponseDto
    {
        public long PricingRuleId { get; set; }
        public string? Description { get; set; }
        public FlowerStatus? flowerstatus { get; set; }
        public string? SpecialDay { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal PriceMultiplier { get; set; }
        public decimal? FixedPrice { get; set; }
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public bool IsGlobal { get; set; }
        public string? CreatedByUserName { get; set; }
    }
}