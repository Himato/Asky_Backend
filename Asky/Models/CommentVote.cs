
using System.ComponentModel.DataAnnotations;

namespace Asky.Models
{
    public class CommentVote
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public bool IsUp { get; set; }

        [Required]
        public int CommentId { get; set; }

        [Required]
        public string UserId { get; set; }

        public Comment Comment { get; set; }

        public ApplicationUser User { get; set; }
    }
}
