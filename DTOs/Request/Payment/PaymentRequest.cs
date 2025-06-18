using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.Payment
{
    public class PaymentRequest
    {
        public long OrderId { get; set; }
        public long PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public PaymentMethod Method { get; set; }
        public BankCode BankCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public Currency Currency { get; set; }
        public DisplayLanguage Language { get; set; }
    }
}