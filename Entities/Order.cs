using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class Order
    {
        public long Id { get; set; }
        [BindNever] public ICollection<OrderItem> OrderItems { get; set; }


        public ApplicationUser User { get; set; }
        public long? UserId { get; set; }

        [Required(ErrorMessage = "Please enter the address to ship to")]
        public Address Address { get; set; }
        public long AddressId { get; set; }
        public string TrackingNumber { get; set; }
        public ShippingStatus OrderStatus { get; set; }
        [NotMapped] public decimal Sum { get; set; }

        public Payment Payment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}