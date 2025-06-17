using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class OrderItem
    {
        public long Id { get; set; }
        public Product Product { get; set; }
        public long ProductId { get; set; }
        public Order Order { get; set; }
        public long OrderId { get; set; }
        public long Quantity { get; set; }
        public int Price { get; set; }
        public ApplicationUser User { get; set; }

        public long? UserId { get; set; }
        public string Name { get; set; }
    }
}