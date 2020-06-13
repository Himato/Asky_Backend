using System;
using Asky.Models;

namespace Asky.Dtos
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public NotificationType Type { get; set; }

        public ViewUserDto Sender { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Uri { get; set; }

        public string Title { get; set; }

        public int TopicId { get; set; }

        public bool IsRead { get; set; }

        public int? Others { get; set; }

        public int? CommentId { get; set; }

        public int? NewId { get; set; }

        public static NotificationDto Create(Notification notification, ApplicationUser sender, Topic topic, Comment comment = null)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Sender = ViewUserDto.Create(sender),
                CreatedAt = notification.CreatedAt,
                Uri = topic.Uri,
                Title = notification.CommentId != null ? "Comment: " + comment?.Content : "Topic: " + topic.Title,
                TopicId = notification.TopicId ?? default,
                IsRead = notification.IsRead,
                CommentId = notification.CommentId,
                NewId = notification.NewId
            };
        }

        public static NotificationDto Create(Notification notification, int others)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Sender = ViewUserDto.Create(notification.Sender),
                CreatedAt = notification.CreatedAt,
                Uri = notification.Topic.Uri,
                Title = notification.CommentId != null ? "Comment: " + notification.Comment.Content : "Topic: " + notification.Topic.Title,
                TopicId = notification.TopicId ?? default,
                IsRead = notification.IsRead,
                Others = others,
                CommentId = notification.CommentId,
                NewId = notification.NewId
            };
        }
    }
}
