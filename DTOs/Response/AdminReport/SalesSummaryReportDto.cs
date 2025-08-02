
namespace BackEnd_FLOWER_SHOP.DTO.Response.Report
{
    public class SalesSummaryReportDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}