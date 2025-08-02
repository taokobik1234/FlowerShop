using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class ApplicationUser : IdentityUser<long>
    {
        public ApplicationUser()
        {

        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public long? RoleId { get; set; }
        public virtual ApplicationRole Role { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Order> Orders { get; set; }

        // New property for loyalty points
        [Column(TypeName = "decimal(18, 2)")]
        public decimal LoyaltyPoints { get; set; } = 0.00M;
    }
}