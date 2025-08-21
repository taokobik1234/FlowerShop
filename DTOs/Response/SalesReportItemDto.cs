namespace FlowerShop.DTOs.Response
{
    public class SalesReportItemDto
    {
        public int Period { get; set; } // Ngày hoặc tháng
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
    }
}
