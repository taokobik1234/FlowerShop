using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.Entities
{
    public class Payment
    {
        public long Id { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; }

        public PaymentMethod Method { get; set; }

        public PaymentStatus Status { get; set; }

        public decimal Amount { get; set; }

        public long TransactionId { get; set; } // For gateway payments

        public string PaymentDetails { get; set; } // JSON or additional payment info

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}