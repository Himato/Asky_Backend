using System;

namespace Asky.Models
{
    public enum NotificationType
    {
        UpVote,
        DownVote,
        Comment,
        CommentUpVote,
        CommentDownVote,
        Reply,
    }

    public class Notification
    {
        public int Id { get; set; }

        public NotificationType Type { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }

        public string SenderId { get; set; }

        public string ReceiverId { get; set; }

        public int? TopicId { get; set; }

        public int? CommentId { get; set; }

        // Holds comment or reply ids
        public int? NewId { get; set; }

        public ApplicationUser Sender { get; set; }

        public ApplicationUser Receiver { get; set; }

        public Topic Topic { get; set; }

        public Comment Comment { get; set; }

        public Notification()
        {
            CreatedAt = DateTime.UtcNow;
            IsRead = false;
        }
    }
}
