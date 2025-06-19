using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request.Payment;
using BackEnd_FLOWER_SHOP.DTOs.Response.Payment;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface IVnpay
    {
        void Initialize(
            string tmnCode,
            string hashSecret,
            string baseUrl,
            string callbackUrl,
            string version = "2.1.0",
            string orderType = "other");

        string GetPaymentUrl(PaymentRequest request);
        PaymentResult GetPaymentResult(IQueryCollection parameters);
    }
}