using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTOs.Request.PricingRule;
using BackEnd_FLOWER_SHOP.DTOs.Response.PricingRule;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class PricingService : IPricingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PricingService> _logger;

        public PricingService(ApplicationDbContext context, ILogger<PricingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<decimal> CalculateDynamicPriceAsync(long productId, DateTime? requestTime = null)
        {
            var checkTime = requestTime ?? DateTime.UtcNow;

            try
            {
                // Get the product with its base price
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (product == null)
                {
                    throw new ArgumentException($"Product with ID {productId} not found");
                }

                // Get applicable pricing rules
                var applicableRules = await GetApplicableRulesAsync(productId, checkTime);

                if (!applicableRules.Any())
                {
                    return product.BasePrice;
                }

                // Apply the highest priority rule
                var selectedRule = applicableRules.OrderByDescending(r => r.Priority).First();

                decimal finalPrice;
                if (selectedRule.FixedPrice.HasValue)
                {
                    finalPrice = selectedRule.FixedPrice.Value;
                }
                else
                {
                    finalPrice = product.BasePrice * selectedRule.PriceMultiplier;
                }

                _logger.LogInformation($"Dynamic price calculated for product {productId}: {finalPrice} (Rule: {selectedRule.PricingRuleId})");
                return Math.Max(finalPrice, 0); // Ensure price is not negative
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating dynamic price for product {productId}");
                throw;
            }
        }

        public async Task<List<PricingRule>> GetApplicableRulesAsync(long productId, DateTime requestTime)
        {
            // Get rules that are either global or specifically associated with the product
            var rules = await _context.PricingRules
                .Where(r => r.IsGlobal || r.ProductPricingRules.Any(ppr => ppr.ProductId == productId))
                .ToListAsync();

            var applicableRules = new List<PricingRule>();

            foreach (var rule in rules)
            {
                if (IsRuleApplicable(rule, productId, requestTime))
                {
                    applicableRules.Add(rule);
                }
                else
                {
                    _logger.LogInformation($"Rule {rule.PricingRuleId} not applicable");
                }
            }

            return applicableRules;
        }

        private bool IsRuleApplicable(PricingRule rule, long productId, DateTime requestTime)
        {
            // Check date range
            _logger.LogInformation($"Rule {requestTime.Date} not applicable - not startday");
            if (rule.StartDate.HasValue && requestTime.Date < rule.StartDate.Value.Date)
                return false;

            _logger.LogInformation($"Rule {rule.PricingRuleId} not applicable - not end");
            if (rule.EndDate.HasValue && requestTime.Date > rule.EndDate.Value.Date)
                return false;

            // Check time range
            if (rule.StartTime.HasValue && rule.EndTime.HasValue)
            {
                _logger.LogInformation($"Rule {rule.PricingRuleId} not applicable - not time");
                var currentTime = requestTime.TimeOfDay;
                if (currentTime < rule.StartTime.Value || currentTime > rule.EndTime.Value)
                    return false;
            }

            // Check special days
            if (!string.IsNullOrWhiteSpace(rule.SpecialDay))
            {
                if (!IsSpecialDay(rule.SpecialDay.Trim(), requestTime))
                {
                    _logger.LogInformation($"Rule {rule.PricingRuleId} not applicable - not special day");
                    return false;
                }
            }

            // Check product condition
            if (!string.IsNullOrEmpty(rule.Condition))
            {
                if (!IsConditionMet(rule.Condition, productId, requestTime))
                    return false;
            }

            return true;
        }

        private bool IsSpecialDay(string specialDay, DateTime date)
        {
            switch (specialDay.ToLower())
            {
                case "valentine":
                case "valentines":
                    return date.Month == 2 && date.Day == 14;

                case "womens_day":
                case "women_day":
                    return date.Month == 3 && date.Day == 8;

                case "mothers_day":
                    var mothersDay = GetNthWeekdayOfMonth(date.Year, 5, DayOfWeek.Sunday, 2);
                    return date.Date == mothersDay.Date;

                case "christmas":
                    return date.Month == 12 && date.Day == 25;

                case "new_year":
                    return date.Month == 1 && date.Day == 1;

                case "weekend":
                    return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

                default:
                    return false;
            }
        }

        private DateTime GetNthWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek, int occurrence)
        {
            var firstDayOfMonth = new DateTime(year, month, 1);
            var firstWeekday = firstDayOfMonth.AddDays((int)dayOfWeek - (int)firstDayOfMonth.DayOfWeek);

            if (firstWeekday.Month < month)
                firstWeekday = firstWeekday.AddDays(7);

            return firstWeekday.AddDays((occurrence - 1) * 7);
        }

        private bool IsConditionMet(string condition, long productId, DateTime requestTime)
        {
            // Implement your business logic for product conditions
            switch (condition.ToLower())
            {
                case "new":
                    // Check if product was created within last 30 days
                    var product = _context.Products.FirstOrDefault(p => p.Id == productId);
                    return product?.CreatedAt >= requestTime.AddDays(-30);

                case "old":
                    // Check if product is older than 30 days
                    var oldProduct = _context.Products.FirstOrDefault(p => p.Id == productId);
                    return oldProduct?.CreatedAt < requestTime.AddDays(-30);

                case "low_stock":
                    // Check if stock is below certain threshold
                    var stockProduct = _context.Products.FirstOrDefault(p => p.Id == productId);
                    return stockProduct?.StockQuantity < 10; // Adjust threshold as needed

                case "high_demand":
                    // This could be based on order history, views, etc.
                    // Implement based on your tracking mechanism
                    return false;

                default:
                    return true;
            }
        }

        public async Task<PricingRuleResponseDto> GetPricingRuleByIdAsync(long ruleId)
        {
            var rule = await _context.PricingRules
                .Include(r => r.ProductPricingRules)
                    .ThenInclude(ppr => ppr.Product)
                .Include(r => r.CreatedByUser)
                .FirstOrDefaultAsync(r => r.PricingRuleId == ruleId);

            if (rule == null)
            {
                throw new ArgumentException($"Pricing rule with ID {ruleId} not found");
            }

            return MapToResponseDto(rule);
        }
        public async Task<PricingRuleResponseDto> CreatePricingRuleAsync(PricingRuleCreateDto ruleDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate products if specified
                if (ruleDto.ProductIds != null && ruleDto.ProductIds.Any())
                {
                    var existingProductIds = await _context.Products
                        .Where(p => ruleDto.ProductIds.Contains(p.Id))
                        .Select(p => p.Id)
                        .ToListAsync();

                    var missingProductIds = ruleDto.ProductIds.Except(existingProductIds).ToList();
                    if (missingProductIds.Any())
                    {
                        throw new ArgumentException($"Products with IDs [{string.Join(", ", missingProductIds)}] not found");
                    }
                }

                // Create the pricing rule
                var rule = new PricingRule
                {
                    Condition = ruleDto.Condition,
                    SpecialDay = ruleDto.SpecialDay,
                    StartTime = ruleDto.StartTime,
                    EndTime = ruleDto.EndTime,
                    StartDate = ruleDto.StartDate?.Kind == DateTimeKind.Utc
                        ? ruleDto.StartDate
                        : DateTime.SpecifyKind(ruleDto.StartDate ?? DateTime.MinValue, DateTimeKind.Utc),
                    EndDate = ruleDto.EndDate?.Kind == DateTimeKind.Utc
                        ? ruleDto.EndDate
                        : DateTime.SpecifyKind(ruleDto.EndDate ?? DateTime.MinValue, DateTimeKind.Utc),
                    PriceMultiplier = ruleDto.PriceMultiplier,
                    FixedPrice = ruleDto.FixedPrice,
                    Priority = ruleDto.Priority,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = ruleDto.CreatedBy,
                    IsGlobal = ruleDto.IsGlobal
                };

                _context.PricingRules.Add(rule);
                await _context.SaveChangesAsync();

                // Create product-pricing rule relationships if specific products are specified
                if (!ruleDto.IsGlobal && ruleDto.ProductIds != null)
                {
                    var productPricingRules = ruleDto.ProductIds.Select(productId => new ProductPricingRule
                    {
                        ProductId = productId,
                        PricingRuleId = rule.PricingRuleId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.ProductPricingRules.AddRange(productPricingRules);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Return the created rule with associated products
                return await GetPricingRuleByIdAsync(rule.PricingRuleId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating pricing rule");
                throw;
            }
        }
        public async Task<PricingRuleResponseDto> UpdatePricingRuleAsync(long ruleId, PricingRuleCreateDto ruleDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rule = await _context.PricingRules
                    .Include(r => r.ProductPricingRules)
                    .FirstOrDefaultAsync(r => r.PricingRuleId == ruleId);

                if (rule == null)
                {
                    throw new ArgumentException($"Pricing rule with ID {ruleId} not found");
                }

                // Validate products if specified
                if (ruleDto.ProductIds != null && ruleDto.ProductIds.Any())
                {
                    var existingProductIds = await _context.Products
                        .Where(p => ruleDto.ProductIds.Contains(p.Id))
                        .Select(p => p.Id)
                        .ToListAsync();

                    var missingProductIds = ruleDto.ProductIds.Except(existingProductIds).ToList();
                    if (missingProductIds.Any())
                    {
                        throw new ArgumentException($"Products with IDs [{string.Join(", ", missingProductIds)}] not found");
                    }
                }

                // Update rule properties
                rule.Condition = ruleDto.Condition;
                rule.SpecialDay = ruleDto.SpecialDay;
                rule.StartTime = ruleDto.StartTime;
                rule.EndTime = ruleDto.EndTime;
                rule.StartDate = ruleDto.StartDate;
                rule.EndDate = ruleDto.EndDate;
                rule.PriceMultiplier = ruleDto.PriceMultiplier;
                rule.FixedPrice = ruleDto.FixedPrice;
                rule.Priority = ruleDto.Priority;
                rule.IsGlobal = ruleDto.IsGlobal;

                // Remove existing product associations
                _context.ProductPricingRules.RemoveRange(rule.ProductPricingRules);

                // Add new product associations if not global
                if (!ruleDto.IsGlobal && ruleDto.ProductIds != null)
                {
                    var newProductPricingRules = ruleDto.ProductIds.Select(productId => new ProductPricingRule
                    {
                        ProductId = productId,
                        PricingRuleId = rule.PricingRuleId,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.ProductPricingRules.AddRange(newProductPricingRules);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetPricingRuleByIdAsync(rule.PricingRuleId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating pricing rule {ruleId}");
                throw;
            }
        }

        public async Task<bool> DeletePricingRuleAsync(long ruleId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var rule = await _context.PricingRules
                    .Include(r => r.ProductPricingRules)
                    .FirstOrDefaultAsync(r => r.PricingRuleId == ruleId);

                if (rule == null)
                {
                    return false;
                }

                // Remove product associations first
                _context.ProductPricingRules.RemoveRange(rule.ProductPricingRules);

                // Remove the rule
                _context.PricingRules.Remove(rule);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error deleting pricing rule {ruleId}");
                throw;
            }
        }

        public async Task<List<PricingRuleResponseDto>> GetPricingRulesForProductAsync(long productId)
        {
            try
            {
                var rules = await _context.PricingRules
                    .Where(r => r.IsGlobal || r.ProductPricingRules.Any(ppr => ppr.ProductId == productId))
                    .Include(r => r.ProductPricingRules)
                        .ThenInclude(ppr => ppr.Product)
                    .Include(r => r.CreatedByUser)
                    .ToListAsync();

                return rules.Select(MapToResponseDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting pricing rules for product {productId}");
                throw;
            }
        }


        public async Task<List<PricingRuleResponseDto>> GetAllPricingRulesAsync()
        {
            try
            {
                var rules = await _context.PricingRules
                    .Include(r => r.ProductPricingRules)
                        .ThenInclude(ppr => ppr.Product)
                    .Include(r => r.CreatedByUser)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return rules.Select(MapToResponseDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all pricing rules");
                throw;
            }
        }

        private PricingRuleResponseDto MapToResponseDto(PricingRule rule)
        {
            return new PricingRuleResponseDto
            {
                PricingRuleId = rule.PricingRuleId,
                Condition = rule.Condition,
                SpecialDay = rule.SpecialDay,
                StartTime = rule.StartTime,
                EndTime = rule.EndTime,
                StartDate = rule.StartDate,
                EndDate = rule.EndDate,
                PriceMultiplier = rule.PriceMultiplier,
                FixedPrice = rule.FixedPrice,
                Priority = rule.Priority,
                CreatedAt = rule.CreatedAt,
                CreatedBy = rule.CreatedBy,
                IsGlobal = rule.IsGlobal,
                CreatedByUserName = rule.CreatedByUser?.UserName,

            };
        }
    }
}