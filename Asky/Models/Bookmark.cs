using System.ComponentModel.DataAnnotations;

namespace Asky.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TopicId { get; set; }

        [Required]
        public string UserId { get; set; }

        public Topic Topic { get; set; }

        public ApplicationUser User { get; set; }
    }
}
