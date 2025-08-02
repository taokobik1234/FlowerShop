namespace BackEnd_FLOWER_SHOP.DTO.Response.Report
{
    public class BestSellingProductReportDto
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}