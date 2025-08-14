using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTOs.Request.Payment;
using BackEnd_FLOWER_SHOP.DTOs.Response.Payment;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IVnpay _vnpayService;

        public PaymentService(ApplicationDbContext context, IVnpay vnpayService)
        {
            _context = context;
            _vnpayService = vnpayService;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, string ipAddress)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate order exists
                var order = await _context.Orders
                    .Include(o => o.Payment)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                    throw new ArgumentException("Order not found");

                if (order.Payment != null)
                    throw new InvalidOperationException("Order already has a payment");

                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Method = request.Method,
                    Status = PaymentStatus.Pending,
                    Amount = request.Amount,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    TransactionId = null
                };

                // Set initial payment details
                var initialDetails = new
                {
                    Description = request.Description,
                    CreatedAt = payment.CreatedAt,
                    Method = payment.Method.ToString(),
                    InitialStatus = "Pending",
                    IpAddress = ipAddress
                };
                payment.PaymentDetails = JsonConvert.SerializeObject(initialDetails);

                // Save payment to database first
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Generate payment URL if needed (for VNPAY)
                string paymentUrl = null;
                if (request.Method == PaymentMethod.VNPay)
                {
                    var vnpayRequest = new PaymentRequest
                    {
                        PaymentId = payment.Id,
                        Amount = request.Amount,
                        Description = request.Description,
                        IpAddress = ipAddress,
                        BankCode = BankCode.ANY,
                        CreatedDate = DateTime.Now,
                        Currency = Currency.VND,
                        Language = DisplayLanguage.Vietnamese
                    };

                    paymentUrl = _vnpayService.GetPaymentUrl(vnpayRequest);

                    if (string.IsNullOrEmpty(paymentUrl))
                    {
                        throw new InvalidOperationException("Failed to generate VNPay payment URL");
                    }
                }

                // Commit transaction only if everything succeeded
                await transaction.CommitAsync();

                return new PaymentResponse
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Method = payment.Method,
                    Status = payment.Status,
                    Amount = payment.Amount,
                    PaymentUrl = paymentUrl,
                    TransactionId = payment.TransactionId,
                    CreatedAt = payment.CreatedAt
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PaymentResponse> ProcessVnpayCallbackAsync(VnpayCallbackRequest request)
        {
            var paymentResult = _vnpayService.GetPaymentResult(request.Query);

            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentResult.PaymentId);

            if (payment == null)
                throw new ArgumentException("Payment not found");

            if (payment.Method != PaymentMethod.VNPay)
                throw new InvalidOperationException("Payment method mismatch");

            if (paymentResult.IsSuccess)
            {
                payment.Status = PaymentStatus.Completed;
                payment.TransactionId = paymentResult.VnpayTransactionId;
                payment.PaymentDetails = JsonConvert.SerializeObject(paymentResult);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.PaymentDetails = JsonConvert.SerializeObject(paymentResult);
            }

            payment.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Method = payment.Method,
                Status = payment.Status,
                Amount = payment.Amount,
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }

        public async Task<PaymentResponse> GetPaymentByIdAsync(long paymentId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
                throw new ArgumentException("Payment not found");

            return new PaymentResponse
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Method = payment.Method,
                Status = payment.Status,
                Amount = payment.Amount,
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }

        public async Task<PaymentResponse> UpdateCodPaymentStatusAsync(long paymentId, PaymentStatus status)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && p.Method == PaymentMethod.COD);

            if (payment == null)
                throw new ArgumentException("COD Payment not found");

            payment.Status = status;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new PaymentResponse
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Method = payment.Method,
                Status = payment.Status,
                Amount = payment.Amount,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }
}