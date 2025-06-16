using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.Product
{
    public class ProductListingResponseDto
    {
        public List<ProductSummaryDto> Products { get; set; } = new List<ProductSummaryDto>();
        public PaginationMetadata Pagination { get; set; }
    }
}