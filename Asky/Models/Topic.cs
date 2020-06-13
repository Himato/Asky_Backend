using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Asky.Models
{
    public class Topic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Uri { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public string UserId { get; set; }

        public Category Category { get; set; }

        public ApplicationUser User { get; set; }

        public List<Vote> Votes { get; }

        public List<View> Views { get; }

        public List<Comment> Comments { get; set; }

        public Topic()
        {
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
            Votes = new List<Vote>();
            Views = new List<View>();
            Comments = new List<Comment>();
        }
    }
}
