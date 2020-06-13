using System.ComponentModel.DataAnnotations;
using Asky.Models;

namespace Asky.Dtos
{
    public class CategoryDto
    {
        [Required]
        [MinLength(4, ErrorMessage = "The name of the category can't be less than 4 characters")]
        public string Name { get; set; }

        [Required]
        [StringLength(6, ErrorMessage = "Invalid Hex Color Value")]
        [RegularExpression(@"^[a-f0-9\s]+$", ErrorMessage = "Invalid Hex Color Value")]
        public string Color { get; set; }
    }

    public class ViewCategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public static ViewCategoryDto Create(Category category)
        {
            return new ViewCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Color = category.Color
            };
        }
    }

    public class AdminCategoryDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public int NumberOfTopics { get; set; }

        public static AdminCategoryDto Create(Category category)
        {
            return new AdminCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Color = category.Color,
                NumberOfTopics = category.Topics.Count
            };
        }
    }
}
