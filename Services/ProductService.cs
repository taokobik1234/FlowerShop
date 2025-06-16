using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductService(
            ApplicationDbContext context,
            ILogger<ProductService> logger,
            ICloudinaryService cloudinaryService)
        {
            _context = context;
            _logger = logger;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto productDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate categories exist
                var existingCategories = await _context.Categories
                    .Where(c => productDto.CategoryIds.Contains(c.Id))
                    .ToListAsync();

                if (existingCategories.Count != productDto.CategoryIds.Count)
                {
                    var missingIds = productDto.CategoryIds.Except(existingCategories.Select(c => c.Id));
                    throw new ArgumentException($"Categories not found: {string.Join(", ", missingIds)}");
                }

                // Create product entity
                var product = new Product
                {
                    Name = productDto.Name,
                    flowerstatus = productDto.FlowerStatus,
                    Description = productDto.Description,
                    BasePrice = productDto.BasePrice,
                    Condition = productDto.Condition,
                    StockQuantity = productDto.StockQuantity,
                    IsActive = productDto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add product to context
                _context.Products.Add(product);
                await _context.SaveChangesAsync(); // Save to get the product ID

                // Upload images to Cloudinary
                var imageUploads = new List<ImageUpload>();
                if (productDto.Images?.Any() == true)
                {
                    var uploadResults = await _cloudinaryService.UploadMultipleImagesAsync(productDto.Images);

                    foreach (var (url, publicId) in uploadResults)
                    {
                        var imageUpload = new ImageUpload
                        {
                            ImageUrl = url,
                            PublicId = publicId,
                            ProductId = product.Id
                        };
                        imageUploads.Add(imageUpload);
                    }

                    _context.ImageUploads.AddRange(imageUploads);
                }

                // Create product-category relationships
                var productCategories = existingCategories.Select(category => new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = category.Id
                }).ToList();

                _context.ProductCategories.AddRange(productCategories);

                // Save all changes
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Product created successfully with ID: {product.Id}");

                // Return response DTO
                return new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    FlowerStatus = product.flowerstatus,
                    Description = product.Description,
                    BasePrice = product.BasePrice,
                    Condition = product.Condition,
                    StockQuantity = product.StockQuantity,
                    IsActive = product.IsActive,
                    Images = imageUploads.Select(img => new ImageResponseDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        PublicId = img.PublicId
                    }).ToList(),
                    Categories = existingCategories.Select(cat => new CategoryResponseDto
                    {
                        Id = cat.Id,
                        Name = cat.Name // Assuming Category has Name property
                    }).ToList(),
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error creating product: {productDto.Name}");

                // Clean up uploaded images if product creation fails
                if (productDto.Images?.Any() == true)
                {
                    try
                    {
                        var uploadResults = await _cloudinaryService.UploadMultipleImagesAsync(productDto.Images);
                        foreach (var (_, publicId) in uploadResults)
                        {
                            await _cloudinaryService.DeleteImageAsync(publicId);
                        }
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Failed to clean up uploaded images after product creation failure");
                    }
                }

                throw;
            }
        }

        // Placeholder for the original CreateProductAsync method
        public async Task<Product> CreateProductAsync(Product product)
        {
            try
            {
                product.CreatedAt = DateTime.UtcNow;
                product.UpdatedAt = DateTime.UtcNow;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Product created successfully with ID: {product.Id}");
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating product: {product.Name}");
                throw;
            }
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(long id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.ImageUploads)
                    .Include(p => p.ProductCategories)
                        .ThenInclude(pc => pc.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogInformation($"Product with ID {id} not found");
                    return null;
                }

                return new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    FlowerStatus = product.flowerstatus,
                    Description = product.Description,
                    BasePrice = product.BasePrice,
                    Condition = product.Condition,
                    StockQuantity = product.StockQuantity,
                    IsActive = product.IsActive,
                    Images = product.ImageUploads?.Select(img => new ImageResponseDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        PublicId = img.PublicId
                    }).ToList() ?? new List<ImageResponseDto>(),
                    Categories = product.ProductCategories?.Select(pc => new CategoryResponseDto
                    {
                        Id = pc.Category.Id,
                        Name = pc.Category.Name
                    }).ToList() ?? new List<CategoryResponseDto>(),
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product with ID: {id}");
                throw;
            }
        }

        public async Task<ProductResponseDto> UpdateProductAsync(long id, ProductCreateDto productDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.ImageUploads)
                    .Include(p => p.ProductCategories)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    throw new ArgumentException($"Product with ID {id} not found");
                }

                // Validate categories exist
                var existingCategories = await _context.Categories
                    .Where(c => productDto.CategoryIds.Contains(c.Id))
                    .ToListAsync();

                if (existingCategories.Count != productDto.CategoryIds.Count)
                {
                    var missingIds = productDto.CategoryIds.Except(existingCategories.Select(c => c.Id));
                    throw new ArgumentException($"Categories not found: {string.Join(", ", missingIds)}");
                }

                // Update product properties
                product.Name = productDto.Name;
                product.flowerstatus = productDto.FlowerStatus;
                product.Description = productDto.Description;
                product.BasePrice = productDto.BasePrice;
                product.Condition = productDto.Condition;
                product.StockQuantity = productDto.StockQuantity;
                product.IsActive = productDto.IsActive;
                product.UpdatedAt = DateTime.UtcNow;

                // Handle image updates
                if (productDto.Images?.Any() == true)
                {
                    // Delete existing images
                    foreach (var image in product.ImageUploads.ToList())
                    {
                        await _cloudinaryService.DeleteImageAsync(image.PublicId);
                        _context.ImageUploads.Remove(image);
                    }

                    // Upload new images
                    var uploadResults = await _cloudinaryService.UploadMultipleImagesAsync(productDto.Images);
                    product.ImageUploads = uploadResults.Select(result => new ImageUpload
                    {
                        ImageUrl = result.Url,
                        PublicId = result.PublicId,
                        ProductId = product.Id
                    }).ToList();

                    _context.ImageUploads.AddRange(product.ImageUploads);
                }

                // Update product categories
                _context.ProductCategories.RemoveRange(product.ProductCategories);
                product.ProductCategories = existingCategories.Select(category => new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = category.Id
                }).ToList();
                _context.ProductCategories.AddRange(product.ProductCategories);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Product updated successfully with ID: {product.Id}");

                return new ProductResponseDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    FlowerStatus = product.flowerstatus,
                    Description = product.Description,
                    BasePrice = product.BasePrice,
                    Condition = product.Condition,
                    StockQuantity = product.StockQuantity,
                    IsActive = product.IsActive,
                    Images = product.ImageUploads.Select(img => new ImageResponseDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        PublicId = img.PublicId
                    }).ToList(),
                    Categories = existingCategories.Select(cat => new CategoryResponseDto
                    {
                        Id = cat.Id,
                        Name = cat.Name
                    }).ToList(),
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating product with ID: {id}");
                throw;
            }
        }

        public async Task<ApiResponse> DeleteProductAsync(long id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .Include(p => p.ImageUploads)
                    .Include(p => p.ProductCategories) // Added to load ProductCategories
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found",
                        Errors = new List<string> { "Product not found" }
                    };
                }

                // Delete images from Cloudinary
                if (product.ImageUploads != null && product.ImageUploads.Any())
                {
                    foreach (var image in product.ImageUploads.ToList())
                    {
                        await _cloudinaryService.DeleteImageAsync(image.PublicId);
                    }
                    _context.ImageUploads.RemoveRange(product.ImageUploads);
                }

                // Delete product categories
                if (product.ProductCategories != null && product.ProductCategories.Any())
                {
                    _context.ProductCategories.RemoveRange(product.ProductCategories);
                }

                // Delete the product
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Product deleted successfully with ID: {id}");
                return new ApiResponse
                {
                    Success = true,
                    Message = "Product deleted successfully"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error deleting product with ID: {id}");
                return new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting the product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<bool> ExistProductAsync(long id)
        {
            return await _context.Products
                                 .AnyAsync(p => p.Id == id);
        }

    }
}