using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class ProductPricingRule
    {
        public long ProductId { get; set; }
        public long PricingRuleId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Product Product { get; set; }
        public virtual PricingRule PricingRule { get; set; }
    }
}