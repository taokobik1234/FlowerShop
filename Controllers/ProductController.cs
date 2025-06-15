using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request;
using BackEnd_FLOWER_SHOP.DTOs.Request.Product;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IProductService _productService; // Assuming you have this service
        private readonly ILogger<ProductController> _logger;

        public ProductController(ICloudinaryService cloudinaryService, IProductService productService, ILogger<ProductController> logger)
        {
            _cloudinaryService = cloudinaryService;
            _productService = productService;
            _logger = logger;

        }
        [HttpPost]
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
                    }); ;
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

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product with ID: {id}");
                return StatusCode(500, "An error occurred while retrieving the product");
            }
        }
    }
}