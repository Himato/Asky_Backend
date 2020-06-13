using System.ComponentModel.DataAnnotations;

namespace Asky.Models
{
    public class View
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TopicId { get; set; }

        public string UserId { get; set; }

        public Topic Topic { get; set; }

        public ApplicationUser User { get; set; }
    }
}
