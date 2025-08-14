using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request.Payment;
using BackEnd_FLOWER_SHOP.DTOs.Response.Payment;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, string ipAddress);
        Task<PaymentResponse> ProcessVnpayCallbackAsync(VnpayCallbackRequest request);
        Task<PaymentResponse> GetPaymentByIdAsync(long paymentId);
        Task<PaymentResponse> UpdateCodPaymentStatusAsync(long paymentId, PaymentStatus status);
    }
}