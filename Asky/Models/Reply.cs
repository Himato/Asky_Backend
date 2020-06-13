using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asky.Models
{
    public class Reply
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int CommentId { get; set; }

        [Required]
        public string UserId { get; set; }

        public Comment Comment { get; set; }

        public ApplicationUser User { get; set; }

        public Reply()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
