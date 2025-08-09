using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.PricingRule
{
    public class PricingRuleCreateDto
    {
        [Required]
        public string Description { get; set; }
        [StringLength(100)]
        public FlowerStatus? flowerstatus { get; set; }

        [StringLength(50)]
        public string? SpecialDay { get; set; } // "valentine", "womens_day", "weekend", etc.

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Required]
        [Range(0.01, 100.0, ErrorMessage = "Price multiplier must be between 0.01 and 100")]
        public decimal PriceMultiplier { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Fixed price must be greater than 0")]
        public decimal? FixedPrice { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Priority must be between 1 and 1000")]
        public int Priority { get; set; }

        // List of product IDs - if null or empty, the rule is global
        public List<long>? ProductIds { get; set; }

        // Helper property to determine if this is a global rule
        public bool IsGlobal => ProductIds == null || !ProductIds.Any();
    }
}