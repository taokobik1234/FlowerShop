using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class PricingRule
    {
        public long PricingRuleId { get; set; }
        public string? Condition { get; set; }
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
        public bool IsGlobal { get; set; } // True if applies to all products

        // Many-to-many relationship with Product
        public virtual ICollection<ProductPricingRule> ProductPricingRules { get; set; } = new List<ProductPricingRule>();
        public virtual ApplicationUser CreatedByUser { get; set; }
    }
}