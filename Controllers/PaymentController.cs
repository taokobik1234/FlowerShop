using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.Payment;
using BackEnd_FLOWER_SHOP.DTOs.Response.Payment;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using BackEnd_FLOWER_SHOP.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IUserService _userService;

        public PaymentController(IPaymentService paymentService, IUserService userService)
        {
            _paymentService = paymentService;
            _userService = userService;
        }

        /// <summary>
        /// Create a new payment for an order
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PaymentResponse>> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid input data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }
                // Get user IP address for online payments
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);

                var payment = await _paymentService.CreatePaymentAsync(request, ipAddress);
                if (payment.Method == PaymentMethod.VNPay && !string.IsNullOrEmpty(payment.PaymentUrl))
                {
                    return Ok(new ApiResponse<PaymentResponse>
                    {
                        Success = true,
                        Message = "Payment created successfully",
                        Data = payment
                    });
                }

                return CreatedAtAction(nameof(GetPayment),
                    new { paymentId = payment.PaymentId },
                    new ApiResponse<PaymentResponse>
                    {
                        Success = true,
                        Message = "Payment created successfully",
                        Data = payment
                    });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get payment details by ID
        /// </summary>
        [HttpGet("{paymentId}")]
        public async Task<ActionResult<PaymentResponse>> GetPayment(long paymentId)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                return Ok(new ApiResponse<PaymentResponse>
                {
                    Success = true,
                    Message = "Payment retrieved successfully",
                    Data = payment
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}