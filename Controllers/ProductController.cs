using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;
using BackEnd_FLOWER_SHOP.DTOs.Response;
using BackEnd_FLOWER_SHOP.DTOs.Response.Product;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IProductService _productService;
        private readonly IUserService _userService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ICloudinaryService cloudinaryService, IProductService productService, ILogger<ProductController> logger, IUserService userService)
        {
            _cloudinaryService = cloudinaryService;
            _productService = productService;
            _userService = userService;
            _logger = logger;
        }

        // AddReview action
        [HttpPost("{id}/reviews")]
        [Authorize] // Ensure only authenticated users can add reviews
        public async Task<IActionResult> AddReview(long id, [FromBody] ReviewCreateDto reviewCreateDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid product ID",
                    });
                }

                // Ensure the DTO product ID matches the route parameter
                if (id != reviewCreateDto.ProductId)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Product ID in route and body must match.",
                    });
                }

                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "Unauthorized access.",
                    });
                }

                var review = await _productService.AddReviewAsync(userId, reviewCreateDto);

                if (review == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found.",
                    });
                }

                return CreatedAtAction(nameof(GetProduct), new { id = reviewCreateDto.ProductId }, review);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding review for product with ID: {id}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while adding the review.",
                });
            }
        }

        // CreateProduct action
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can create products
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid input data",
                        Errors = errors
                    });
                }

                var result = await _productService.CreateProductAsync(productDto);
                return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, new ApiResponse<ProductResponseDto>
                {
                    Success = true,
                    Message = "Product created successfully",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input for product creation");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "An error occurred while creating the product" });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // Allow all users including guests to view individual products
        public async Task<IActionResult> GetProduct(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid product ID",
                    });
                }

                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = $"Product with ID {id} not found",
                        Errors = new List<string> { "Product not found" }
                    });
                }

                var userId = _userService.GetUserId();
                if (userId.HasValue)
                {
                    await _productService.TrackProductViewAsync(userId.Value, id);
                }


                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product with ID: {id}");
                return StatusCode(500, "An error occurred while retrieving the product");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can update products
        public async Task<IActionResult> UpdateProduct(long id, [FromForm] ProductCreateDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid input data",
                        Errors = errors
                    });
                }

                var result = await _productService.UpdateProductAsync(id, productDto);
                return Ok(new ApiResponse<ProductResponseDto>
                {
                    Success = true,
                    Message = "Product updated successfully",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid input for updating product with ID: {id}");
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with ID: {id}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while updating the product",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete products
        public async Task<IActionResult> DeleteProduct(long id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result.Success)
                {
                    return StatusCode(result.Errors.Any() ? 400 : 404, result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with ID: {id}");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while deleting the product",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet]
        [AllowAnonymous] // Allow all users including guests to browse products
        public async Task<IActionResult> GetProducts([FromQuery] ProductListingRequestDto request)
        {
            try
            {
                // Validate pagination parameters
                if (request.Page < 1)
                    request.Page = 1;

                if (request.PageSize < 1 || request.PageSize > 100)
                    request.PageSize = 10;

                // Validate price range
                if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice > request.MaxPrice)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "Minimum price cannot be greater than maximum price",
                        Errors = new List<string> { "Invalid price range" }
                    });
                }

                var result = await _productService.GetProductListingsAsync(request);

                return Ok(new ApiResponse<ProductListingResponseDto>
                {
                    Success = true,
                    Message = "Products retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product listings");
                return StatusCode(500, new ApiResponse
                {
                    Success = false,
                    Message = "An error occurred while retrieving products",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost("{productId}/track-view")]
        [Authorize]
        public async Task<IActionResult> TrackView(long productId)
        {
            var userId = _userService.GetUserId();
            if (!userId.HasValue) return Unauthorized();
            await _productService.TrackProductViewAsync(userId.Value, productId);
            return Ok(new { message = "View tracked successfully" });
        }

        [HttpGet("recommendations/for-you")]
        [Authorize]
        public async Task<IActionResult> GetRecommendationsForUser([FromQuery] int count = 6)
        {
            var userId = _userService.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var recommendations = await _productService.GetRecommendationsForUserAsync(userId.Value, count);
            return Ok(new
            {
                products = recommendations,
                type = "Personalized",
                count = recommendations.Count
            });
        }

        [HttpGet("recommendations/popular")]
        public async Task<IActionResult> GetPopularProducts([FromQuery] int count = 6)
        {
            var popularProducts = await _productService.GetPopularProductsAsync(count);
            return Ok(new
            {
                products = popularProducts,
                type = "Popular",
                count = popularProducts.Count
            });
        }

        [HttpGet("{productId}/similar")]
        public async Task<IActionResult> GetSimilarProducts(long productId, [FromQuery] int count = 6)
        {
            var similarProducts = await _productService.GetSimilarProductsAsync(productId, count);
            return Ok(new
            {
                products = similarProducts,
                type = "Similar",
                count = similarProducts.Count
            });
        }

        [HttpGet("recommendations/recently-viewed")]
        [Authorize]
        public async Task<IActionResult> GetRecentlyViewed([FromQuery] int count = 6)
        {
            var userId = _userService.GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var recentlyViewed = await _productService.GetRecentlyViewedAsync(userId.Value, count);
            return Ok(new
            {
                products = recentlyViewed,
                type = "Recently Viewed",
                count = recentlyViewed.Count
            });
        }
    }
}