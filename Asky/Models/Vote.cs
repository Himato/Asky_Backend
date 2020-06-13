
using System.ComponentModel.DataAnnotations;

namespace Asky.Models
{
    public class Vote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public bool IsUp { get; set; }

        [Required]
        public int TopicId { get; set; }

        [Required]
        public string UserId { get; set; }

        public Topic Topic { get; set; }

        public ApplicationUser User { get; set; }
    }
}
