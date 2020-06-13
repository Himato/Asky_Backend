using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asky.Dtos;
using Asky.Helpers;
using Asky.Models;
using Microsoft.EntityFrameworkCore;

namespace Asky.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAll();

        IEnumerable<AdminCategoryDto> GetCategoriesForAdmin();

        Task<Category> GetCategory(int id);

        Task<Category> AddCategory(CategoryDto categoryDto);

        Task<Category> UpdateCategory(int id, CategoryDto categoryDto);

        Task DeleteCategory(int id);
    }

    public class CategoryService : ServiceHelper, ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAll()
        {
            return await GetOrderedCategories().ToListAsync();
        }

        public IEnumerable<AdminCategoryDto> GetCategoriesForAdmin()
        {
            return GetOrderedCategories()
                .Include(c => c.Topics)
                .Select(AdminCategoryDto.Create)
                .ToList();
        }

        public async Task<Category> GetCategory(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category> AddCategory(CategoryDto categoryDto)
        {
            if (await DoesCategoryExist(categoryDto.Name))
            {
                throw new ArgumentException("This name already exists");
            }

            var category = new Category
            {
                Name = categoryDto.Name,
                Uri = categoryDto.Name.GetUniqueUri(),
                Color = categoryDto.Color
            };

            await Do(async () => await _context.Categories.AddAsync(category));

            return category;
        }

        public async Task<Category> UpdateCategory(int id, CategoryDto categoryDto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            if (category.Name.Equals(categoryDto.Name))
            {
                await Do(() =>
                {
                    category.Color = categoryDto.Color;
                    _context.Entry(category).State = EntityState.Modified;
                });

                return category;
            }

            if (await DoesCategoryExist(categoryDto.Name))
            {
                throw new ArgumentException("This name already exists");
            }

            await Do(() =>
            {
                category.Name = categoryDto.Name;
                category.Uri = categoryDto.Name.GetUniqueUri();
                category.Color = categoryDto.Color;
                _context.Entry(category).State = EntityState.Modified;
            });

            return category;
        }

        public async Task DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                throw new KeyNotFoundException("Category not found");
            }

            if (await _context.Topics.AnyAsync(t => t.CategoryId == id))
            {
                throw new KeyNotFoundException("Cannot delete this category since it has topics under it");
            }

            await Do(() => _context.Categories.Remove(category));
        }

        private IQueryable<Category> GetOrderedCategories()
        {
            return _context.Categories.OrderBy(c => c.Name).AsQueryable();
        }

        private async Task<bool> DoesCategoryExist(string categoryName)
        {
            return await _context.Categories.AnyAsync(s => s.Name.Equals(categoryName.Trim()));
        }
    }
}
