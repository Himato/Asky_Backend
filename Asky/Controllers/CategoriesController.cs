using System.Threading.Tasks;
using Asky.Dtos;
using Asky.Helpers;
using Asky.Models;
using Asky.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asky.Controllers
{
    [Authorize(Policy = nameof(UserRole.Admin))]
    public class CategoriesController : ApiHelper
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            return await Do(async () => await _categoryService.GetAll());
        }

        [HttpGet]
        [Route("Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetCategoriesForAdmin(int id = 0)
        {
            if (id == 0)
            {
                return Do(() => _categoryService.GetCategoriesForAdmin());
            }

            return await Do(async () => await _categoryService.GetCategory(id));
        }

        [HttpPost]
        [Route("Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AddCategory([FromBody] CategoryDto categoryDto)
        {
            return await Create(nameof(GetCategoriesForAdmin), async () => await _categoryService.AddCategory(categoryDto));
        }

        [HttpPut]
        [Route("Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto categoryDto)
        {
            return await Do(async () => await _categoryService.UpdateCategory(id, categoryDto));
        }

        [HttpDelete]
        [Route("Admin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            return await Do(async () => await _categoryService.DeleteCategory(id));
        }
    }
}