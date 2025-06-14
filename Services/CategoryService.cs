using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Data;
using BackEnd_FLOWER_SHOP.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_FLOWER_SHOP.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context; // Replace with your actual DbContext name

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.ProductCategories)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(long id)
        {
            return await _context.Categories
                .Include(c => c.ProductCategories)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(long id, Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null)
                return null;

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Update(existingCategory);
            await _context.SaveChangesAsync();
            return existingCategory;
        }

        public async Task<bool> DeleteCategoryAsync(long id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CategoryExistsAsync(long id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }
    }
}