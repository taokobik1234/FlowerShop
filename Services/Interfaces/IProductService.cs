using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;
using BackEnd_FLOWER_SHOP.DTOs.Response.Product;
using BackEnd_FLOWER_SHOP.Entities;

namespace BackEnd_FLOWER_SHOP.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto);
        Task<Product> CreateProductAsync(Product product);

        Task<ProductResponseDto?> GetProductByIdAsync(long id);
        Task<ProductResponseDto> UpdateProductAsync(long id, ProductCreateDto productDto); // New method
        Task<ApiResponse> DeleteProductAsync(long id);
        Task<bool> ExistProductAsync(long id);
        Task<ReviewResponseDto?> AddReviewAsync(long userId, ReviewCreateDto reviewCreateDto);
        Task<ProductListingResponseDto> GetProductListingsAsync(ProductListingRequestDto request);

        Task TrackProductViewAsync(long userId, long productId);
        Task<List<ProductSummaryDto>> GetRecommendationsForUserAsync(long userId, int count = 6);
        Task<List<ProductSummaryDto>> GetPopularProductsAsync(int count = 6);
        Task<List<ProductSummaryDto>> GetSimilarProductsAsync(long productId, int count = 6);
        Task<List<ProductSummaryDto>> GetRecentlyViewedAsync(long userId, int count = 6);

        Task<List<ProductSummaryDto>> SearchProductsAsync(string? query);

        Task<List<string>> GetProductSuggestionsAsync(string? query);
    }

}