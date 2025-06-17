using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.PricingRule;
using BackEnd_FLOWER_SHOP.DTOs.Response.PricingRule;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/pricing")]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        private readonly IProductService _productService;
        private readonly ILogger<PricingController> _logger;

        public PricingController(
            IPricingService pricingService,
            IProductService productService,
            ILogger<PricingController> logger)
        {
            _pricingService = pricingService;
            _productService = productService;
            _logger = logger;
        }

        [HttpGet("products/{productId}/price")]
        public async Task<IActionResult> GetDynamicPrice(long productId, [FromQuery] DateTime? requestTime = null)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = $"Product with ID {productId} not found"
                    });
                }

                var checkTime = requestTime ?? DateTime.UtcNow;
                var dynamicPrice = await _pricingService.CalculateDynamicPriceAsync(productId, checkTime);
                var applicableRules = await _pricingService.GetApplicableRulesAsync(productId, checkTime);

                var response = new DynamicPriceResponseDto
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    BasePrice = product.BasePrice,
                    DynamicPrice = dynamicPrice,
                    Discount = product.BasePrice - dynamicPrice,
                    DiscountPercentage = product.BasePrice > 0 ? ((product.BasePrice - dynamicPrice) / product.BasePrice) * 100 : 0,
                    AppliedRule = applicableRules.OrderByDescending(r => r.Priority).FirstOrDefault()?.Condition,
                    CalculatedAt = checkTime
                };

                return Ok(new ApiResponse<DynamicPriceResponseDto>
                {
                    Success = true,
                    Message = "Dynamic price calculated successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating dynamic price for product {productId}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while calculating dynamic price"
                });
            }
        }

        [HttpPost("rules")]
        public async Task<IActionResult> CreatePricingRule([FromBody] PricingRuleCreateDto ruleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid input data",
                        Errors = errors
                    });
                }

                // Validate products exist if specific product IDs are provided
                if (ruleDto.ProductIds != null && ruleDto.ProductIds.Any())
                {
                    foreach (var productId in ruleDto.ProductIds)
                    {
                        var existProduct = await _productService.ExistProductAsync(productId);
                        if (!existProduct)
                        {
                            return NotFound(new ApiResponse
                            {
                                Success = false,
                                Message = $"Product with ID {productId} not found"
                            });
                        }
                    }
                }

                var result = await _pricingService.CreatePricingRuleAsync(ruleDto);
                return CreatedAtAction(nameof(GetPricingRule), new { id = result.PricingRuleId },
                    new ApiResponse<PricingRuleResponseDto>
                    {
                        Success = true,
                        Message = "Pricing rule created successfully",
                        Data = result
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pricing rule");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while creating the pricing rule"
                });
            }
        }

        [HttpGet("rules/{id}")]
        public async Task<IActionResult> GetPricingRule(long id)
        {
            try
            {
                var rule = await _pricingService.GetPricingRuleByIdAsync(id);
                return Ok(new ApiResponse<PricingRuleResponseDto>
                {
                    Success = true,
                    Data = rule
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pricing rule {id}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving the pricing rule"
                });
            }
        }

        [HttpGet("rules")]
        public async Task<IActionResult> GetAllPricingRules()
        {
            try
            {
                var rules = await _pricingService.GetAllPricingRulesAsync();
                return Ok(new ApiResponse<List<PricingRuleResponseDto>>
                {
                    Success = true,
                    Data = rules,
                    Message = $"Retrieved {rules.Count} pricing rules"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all pricing rules");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving pricing rules"
                });
            }
        }

        [HttpGet("products/{productId}/rules")]
        public async Task<IActionResult> GetProductPricingRules(long productId)
        {
            try
            {
                var rules = await _pricingService.GetPricingRulesForProductAsync(productId);
                return Ok(new ApiResponse<List<PricingRuleResponseDto>>
                {
                    Success = true,
                    Data = rules,
                    Message = $"Retrieved {rules.Count} pricing rules for product {productId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving pricing rules for product {productId}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving pricing rules"
                });
            }
        }

        [HttpPut("rules/{id}")]
        public async Task<IActionResult> UpdatePricingRule(long id, [FromBody] PricingRuleCreateDto ruleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid input data",
                        Errors = errors
                    });
                }

                // Validate products exist if specific product IDs are provided
                if (ruleDto.ProductIds != null && ruleDto.ProductIds.Any())
                {
                    foreach (var productId in ruleDto.ProductIds)
                    {
                        var existProduct = await _productService.ExistProductAsync(productId);
                        if (!existProduct)
                        {
                            return NotFound(new ApiResponse
                            {
                                Success = false,
                                Message = $"Product with ID {productId} not found"
                            });
                        }
                    }
                }

                var result = await _pricingService.UpdatePricingRuleAsync(id, ruleDto);
                return Ok(new ApiResponse<PricingRuleResponseDto>
                {
                    Success = true,
                    Message = "Pricing rule updated successfully",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating pricing rule {id}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while updating the pricing rule"
                });
            }
        }

        [HttpDelete("rules/{id}")]
        public async Task<IActionResult> DeletePricingRule(long id)
        {
            try
            {
                var success = await _pricingService.DeletePricingRuleAsync(id);
                if (!success)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = $"Pricing rule with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Pricing rule deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting pricing rule {id}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting the pricing rule"
                });
            }
        }

        // [HttpGet("test/special-days")]
        // public IActionResult GetSpecialDaysReference()
        // {
        //     var reference = new
        //     {
        //         SupportedSpecialDays = new[]
        //         {
        //             "valentine", "womens_day", "mothers_day", "christmas", "new_year", "weekend"
        //         },
        //         SupportedConditions = new[]
        //         {
        //             "new", "old", "low_stock", "high_demand"
        //         },
        //         Examples = new[]
        //         {
        //             new { 
        //                 Rule = "Valentine's Day Premium (Global)", 
        //                 SpecialDay = "valentine", 
        //                 PriceMultiplier = 1.5m,
        //                 ProductIds = (List<long>?)null,
        //                 Description = "Applies to all products"
        //             },
        //             new { 
        //                 Rule = "Weekend Discount (Specific Products)", 
        //                 SpecialDay = "weekend", 
        //                 PriceMultiplier = 0.9m,
        //                 ProductIds = new List<long> { 1, 2, 3 },
        //                 Description = "Applies only to products 1, 2, and 3"
        //             },
        //             new { 
        //                 Rule = "New Product Premium", 
        //                 Condition = "new", 
        //                 PriceMultiplier = 1.2m,
        //                 ProductIds = (List<long>?)null,
        //                 Description = "Global rule for new products"
        //             }
        //         }
        //     };

        //     return Ok(new ApiResponse<object>
        //     {
        //         Success = true,
        //         Data = reference,
        //         Message = "Special days and conditions reference with N:N relationship examples"
        //     });
        // }
    }
}