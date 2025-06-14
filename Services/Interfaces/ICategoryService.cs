using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Entities;

namespace BackEnd_FLOWER_SHOP.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(long id);
        Task<Category> CreateCategoryAsync(Category category);
        Task<Category> UpdateCategoryAsync(long id, Category category);
        Task<bool> DeleteCategoryAsync(long id);
        Task<bool> CategoryExistsAsync(long id);
    }
}