using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Asky.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Color { get; set; }

        [Required]
        public string Uri { get; set; }

        public ICollection<Topic> Topics { get; set; }

        public Category()
        {
            Topics = new List<Topic>();
        }
    }
}
