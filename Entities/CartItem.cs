using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class CartItem
    {
        public long Id { get; set; }
        public long CartId { get; set; }
        public long ProductId { get; set; }
        public decimal Price { get; set; }
        public long Quantity { get; set; }

        public virtual Cart Cart { get; set; }
        public virtual Product Product { get; set; }
    }
}