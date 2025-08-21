// File: Services/Interfaces/IReportService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTO.Response.Report;


namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface IReportService
    {
        Task<SalesSummaryReportDto> GetSalesSummaryReportAsync(DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<BestSellingProductReportDto>> GetBestSellingProductsReportAsync(int topN, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<FlowerShop.DTOs.Response.SalesReportItemDto>> GetSalesMonthReportAsync(int month, int year);
    Task<IEnumerable<FlowerShop.DTOs.Response.SalesReportItemDto>> GetSalesYearReportAsync(int year);
    }
}