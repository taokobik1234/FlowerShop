using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class Product
    {
        public Product()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public FlowerType flowerType { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public string Condition { get; set; }
        public long StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }

        public ICollection<ImageUpload> ProductImages { get; set; }
        public virtual ICollection<PricingRule> PricingRules { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}