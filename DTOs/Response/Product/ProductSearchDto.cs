using System.Collections.Generic;
using System.Linq;
using BackEnd_FLOWER_SHOP.DTOs.Response.Product;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.Product
{
    public class ProductSearchDto
    {
        public string? Query { get; set; }
        public int TotalCount { get; set; }
        public List<ProductSummaryDto> Products { get; set; } = new List<ProductSummaryDto>();
    }
}
