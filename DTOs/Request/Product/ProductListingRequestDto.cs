using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Enums;

namespace BackEnd_FLOWER_SHOP.DTOs.Response.Product
{
    public class ProductListingRequestDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Filtering
        public List<FlowerStatus>? FlowerStatuses { get; set; }
        public List<string>? Occasions { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<long>? CategoryIds { get; set; }
        public bool? IsActive { get; set; } = true;
        public string? SearchTerm { get; set; }

        // Sorting
        public ProductSortBy SortBy { get; set; } = ProductSortBy.Newest;
        public SortDirection SortDirection { get; set; } = SortDirection.Descending;
    }
}