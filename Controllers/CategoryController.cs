using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.DTOs.Request.Category;
using BackEnd_FLOWER_SHOP.Entities;
using BackEnd_FLOWER_SHOP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd_FLOWER_SHOP.Controllers
{
    [Route("api/category")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(long id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound($"Category with ID {id} not found.");
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] CategoryDto categoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = new Category
                {
                    Name = categoryDto.Name,
                    Description = categoryDto.Description
                };

                var createdCategory = await _categoryService.CreateCategoryAsync(category);
                return CreatedAtAction(nameof(GetCategory), new { id = createdCategory.Id }, createdCategory);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("Category data is required.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> UpdateCategory(long id, [FromBody] CategoryDto categoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var category = new Category
                {
                    Name = categoryDto.Name,
                    Description = categoryDto.Description
                };

                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, category);
                if (updatedCategory == null)
                {
                    return NotFound($"Category with ID {id} not found.");
                }

                return Ok(updatedCategory);
            }
            catch (ArgumentNullException)
            {
                return BadRequest("Category data is required.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCategory(long id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (!result)
                {
                    return NotFound($"Category with ID {id} not found.");
                }

                return Ok("Delete Category successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }


}