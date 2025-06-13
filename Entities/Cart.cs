using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class Cart
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
}