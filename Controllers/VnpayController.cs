using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request.Payment;
using BackEnd_FLOWER_SHOP.DTOs.Response.Payment;
using BackEnd_FLOWER_SHOP.Enums;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnpayController : ControllerBase
    {
        private readonly IVnpay _vnpay;
        private readonly IConfiguration _configuration;
        private readonly IPaymentService _paymentService;

        public VnpayController(
            IVnpay vnPayservice,
            IConfiguration configuration,
            IPaymentService paymentService)
        {
            _vnpay = vnPayservice;
            _configuration = configuration;
            _paymentService = paymentService;

        }

        /// <summary>
        /// Thực hiện hành động sau khi thanh toán. URL này cần được khai báo với VNPAY để API này hoạt đồng
        /// </summary>
        [HttpGet("IpnAction")]
        public async Task<IActionResult> IpnAction()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var request = new VnpayCallbackRequest
                    {
                        Query = Request.Query
                    };

                    var result = await _paymentService.ProcessVnpayCallbackAsync(request);

                    if (result.Status == PaymentStatus.Completed)
                    {
                        return Ok(result);
                    }

                    return BadRequest(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Not Found Payment infomation.");
        }

        /// <summary>
        /// Trả kết quả thanh toán về cho người dùng
        /// </summary>
        [HttpGet("Callback")]
        public async Task<ActionResult<PaymentResponse>> Callback()
        {
            if (Request.QueryString.HasValue)
            {
                try
                {
                    var request = new VnpayCallbackRequest
                    {
                        Query = Request.Query
                    };

                    var result = await _paymentService.ProcessVnpayCallbackAsync(request);

                    if (result.Status == PaymentStatus.Completed)
                    {
                        return Redirect($"https://flowershop-fe.vercel.app/payment/success?paymentId={result.PaymentId}");
                    }
                    else
                    {
                        return Redirect("https://flowershop-fe.vercel.app/payment/failed");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            return NotFound("Not Found Payment infomation.");
        }
    }
}