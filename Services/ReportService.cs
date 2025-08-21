using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTO.Response.Report;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SalesSummaryReportDto> GetSalesSummaryReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderStatus == ShippingStatus.Delivered);

            if (startDate.HasValue)
            {
                // Ensure date is UTC before querying the database
                var utcStartDate = startDate.Value.ToUniversalTime();
                query = query.Where(o => o.CreatedAt >= utcStartDate);
            }

            if (endDate.HasValue)
            {
                // Ensure date is UTC before querying the database
                var utcEndDate = endDate.Value.ToUniversalTime();
                query = query.Where(o => o.CreatedAt <= utcEndDate);
            }

            var salesSummary = await query
                .GroupBy(o => 1)
                .Select(g => new SalesSummaryReportDto
                {
                    TotalOrders = g.Count(),
                    TotalRevenue = g.Sum(o => o.OrderItems.Sum(oi => oi.Price * oi.Quantity)),
                    AverageOrderValue = g.Any() 
                        ? g.Average(o => (decimal)o.OrderItems.Sum(oi => oi.Price * oi.Quantity))
                        : 0m
                })
                .FirstOrDefaultAsync();

            return salesSummary ?? new SalesSummaryReportDto();
        }

        public async Task<IEnumerable<BestSellingProductReportDto>> GetBestSellingProductsReportAsync(int topN, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderStatus == ShippingStatus.Delivered);

            if (startDate.HasValue)
            {
                // Ensure date is UTC before querying the database
                var utcStartDate = startDate.Value.ToUniversalTime();
                query = query.Where(oi => oi.Order.CreatedAt >= utcStartDate);
            }

            if (endDate.HasValue)
            {
                // Ensure date is UTC before querying the database
                var utcEndDate = endDate.Value.ToUniversalTime();
                query = query.Where(oi => oi.Order.CreatedAt <= utcEndDate);
            }

            var bestSellingProducts = await query
                .GroupBy(oi => new { oi.ProductId, oi.Name })
                .Select(g => new BestSellingProductReportDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalQuantitySold = (int)g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(topN)
                .ToListAsync();

            return bestSellingProducts;
        }

        public async Task<IEnumerable<FlowerShop.DTOs.Response.SalesReportItemDto>> GetSalesMonthReportAsync(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderStatus == ShippingStatus.Delivered && o.CreatedAt.Year == year && o.CreatedAt.Month == month)
                .ToListAsync();

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var report = new List<FlowerShop.DTOs.Response.SalesReportItemDto>();
            for (int day = 1; day <= daysInMonth; day++)
            {
                var dayOrders = orders.Where(o => o.CreatedAt.Day == day).ToList();
                var totalOrders = dayOrders.Count;
                var totalRevenue = dayOrders.Sum(o => o.OrderItems.Sum(oi => oi.Price * oi.Quantity));
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;
                report.Add(new FlowerShop.DTOs.Response.SalesReportItemDto
                {
                    Period = day,
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = averageOrderValue
                });
            }
            return report;
        }

        public async Task<IEnumerable<FlowerShop.DTOs.Response.SalesReportItemDto>> GetSalesYearReportAsync(int year)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.OrderStatus == ShippingStatus.Delivered && o.CreatedAt.Year == year)
                .ToListAsync();

            var report = new List<FlowerShop.DTOs.Response.SalesReportItemDto>();
            for (int month = 1; month <= 12; month++)
            {
                var monthOrders = orders.Where(o => o.CreatedAt.Month == month).ToList();
                var totalOrders = monthOrders.Count;
                var totalRevenue = monthOrders.Sum(o => o.OrderItems.Sum(oi => oi.Price * oi.Quantity));
                var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;
                report.Add(new FlowerShop.DTOs.Response.SalesReportItemDto
                {
                    Period = month,
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = averageOrderValue
                });
            }
            return report;
        }
    }
}
