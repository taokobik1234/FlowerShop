using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request.PricingRule;
using BackEnd_FLOWER_SHOP.DTOs.Response.PricingRule;
using BackEnd_FLOWER_SHOP.Entities;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface IPricingService
    {
        Task<decimal> CalculateDynamicPriceAsync(long productId, DateTime? requestTime = null);
        Task<PricingRuleResponseDto> CreatePricingRuleAsync(PricingRuleCreateDto ruleDto);
        Task<PricingRuleResponseDto> UpdatePricingRuleAsync(long ruleId, PricingRuleCreateDto ruleDto);
        Task<bool> DeletePricingRuleAsync(long ruleId);
        Task<List<PricingRuleResponseDto>> GetPricingRulesForProductAsync(long productId);
        Task<List<PricingRule>> GetApplicableRulesAsync(long productId, DateTime requestTime);
    }
}