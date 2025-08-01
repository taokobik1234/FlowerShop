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
            ImageUploads = new List<ImageUpload>();
            ProductPricingRules = new List<ProductPricingRule>();

        }
        public ICollection<Review> Reviews { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public FlowerStatus flowerstatus { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public string Condition { get; set; }
        public long StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }

        public virtual ICollection<ImageUpload> ImageUploads { get; set; }

        // Many-to-many relationship with PricingRule
        public virtual ICollection<ProductPricingRule> ProductPricingRules { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}