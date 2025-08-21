using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [ApiController]
    [Route("api/admin/reports")]
    // [Authorize(Roles = "Admin")] // Restrict access to users with the "Admin" role
    public class AdminReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public AdminReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Gets a summary of sales data.
        /// </summary>
        /// <param name="startDate">Optional start date to filter by.</param>
        /// <param name="endDate">Optional end date to filter by.</param>
        /// <returns>A sales summary report.</returns>
        [HttpGet("sales-summary")]
        public async Task<IActionResult> GetSalesSummary([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var report = await _reportService.GetSalesSummaryReportAsync(startDate, endDate);
            return Ok(report);
        }

        /// <summary>
        /// Gets a list of the best-selling products.
        /// </summary>
        /// <param name="topN">The number of top products to return.</param>
        /// <param name="startDate">Optional start date to filter by.</param>
        /// <param name="endDate">Optional end date to filter by.</param>
        /// <returns>A list of best-selling products.</returns>
        [HttpGet("best-selling-products")]
        public async Task<IActionResult> GetBestSellingProducts([FromQuery] int topN = 10, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var report = await _reportService.GetBestSellingProductsReportAsync(topN, startDate, endDate);
            return Ok(report);
        }

        /// <summary>
        /// Gets sales report for each day in a month.
        /// </summary>
        /// <param name="month">Month (1-12)</param>
        /// <param name="year">Year</param>
        /// <returns>Array of sales report per day</returns>
        [HttpGet("sales-month")]
        public async Task<IActionResult> GetSalesMonth([FromQuery] int month, [FromQuery] int year)
        {
            var report = await _reportService.GetSalesMonthReportAsync(month, year);
            return Ok(report);
        }

        /// <summary>
        /// Gets sales report for each month in a year.
        /// </summary>
        /// <param name="year">Year</param>
        /// <returns>Array of sales report per month</returns>
        [HttpGet("sales-year")]
        public async Task<IActionResult> GetSalesYear([FromQuery] int year)
        {
            var report = await _reportService.GetSalesYearReportAsync(year);
            return Ok(report);
        }
    }
}
