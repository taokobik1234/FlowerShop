using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class ApplicationRole : IdentityRole<long>
    {
        public ApplicationRole(string name) : base(name)
        {
            CreationDate = DateTime.Now;
            ModificationDate = DateTime.Now;
        }

        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
    }
}