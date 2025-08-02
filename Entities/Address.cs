using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class Address
    {
        public long Id { get; set; }
        public string FullName { get; set; } // Changed from FirstName and LastName
        public string PhoneNumber { get; set; } // New property
        public string StreetAddress { get; set; }
        public string City { get; set; }
        // Country and ZipCode have been removed

        public ApplicationUser User { get; set; }
        public long? ApplicationUserId { get; set; }
    }
}