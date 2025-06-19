using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.Payment
{
    public class PaymentResponseInfo
    {
        public ResponseCode Code { get; set; }
        public string Description { get; set; }
    }
}