using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.PricingRule
{
    public class PricingRuleCreateDto
    {
        public long FlowerId { get; set; } // null for global rules

        [StringLength(100)]
        public string? Condition { get; set; } // "new", "old", "low_stock", etc.

        [StringLength(50)]
        public string? SpecialDay { get; set; } // "valentine", "womens_day", "weekend", etc.

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0.1, 10.0)]
        public decimal PriceMultiplier { get; set; } = 1.0m;

        [Range(0.01, double.MaxValue)]
        public decimal? FixedPrice { get; set; }

        [Range(1, 100)]
        public int Priority { get; set; } = 1;

        [Required]
        public long CreatedBy { get; set; }
    }
}