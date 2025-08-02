using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class UserProductView
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ProductId { get; set; }
        public DateTime ViewedAt { get; set; }
        public int ViewCount { get; set; } = 1;

        public virtual ApplicationUser User { get; set; }
        public virtual Product Product { get; set; }
    }
}