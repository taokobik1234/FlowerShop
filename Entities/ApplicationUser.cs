using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class ApplicationUser : IdentityUser<long>
    {
        public ApplicationUser()
        {

        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? RoleId { get; set; }
        public ImageUpload ProfileImage { get; set; }
        public virtual ApplicationRole Role { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Order> Orders { get; set; }
    }
}