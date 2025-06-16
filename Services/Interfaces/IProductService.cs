using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;
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
    }

}