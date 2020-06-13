using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Asky.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int TopicId { get; set; }

        [Required]
        public string UserId { get; set; }

        public Topic Topic { get; set; }

        public ApplicationUser User { get; set; }

        public List<CommentVote> Votes { get; }

        public List<Reply> Replies { get; set; }

        public Comment()
        {
            CreatedAt = DateTime.UtcNow;
            Votes = new List<CommentVote>();
            Replies = new List<Reply>();
        }
    }
}
