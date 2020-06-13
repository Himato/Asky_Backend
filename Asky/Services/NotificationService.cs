using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asky.Dtos;
using Asky.Hubs;
using Asky.Models;
using Asky.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Asky.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetNotifications(string userId, int? limit);

        Task MarkAsRead(string userId, int notificationId);

        Task NotifyVote(ApplicationUser sender, Topic topic, bool isUp);

        Task NotifyComment(ApplicationUser sender, Topic topic, int commentId);

        Task NotifyCommentVote(ApplicationUser sender, Comment comment, bool isUp);

        Task NotifyReply(ApplicationUser sender, Comment comment, int replyId);
    }

    public class NotificationService : ServiceHelper, INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationsHub> _notificationsHub;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationsHub> notificationsHub) : base(context)
        {
            _context = context;
            _notificationsHub = notificationsHub;
        }

        public async Task<IEnumerable<NotificationDto>> GetNotifications(string userId, int? limit)
        {
            if (limit != null && limit < 0)
            {
                throw new ArgumentException("Limit must be a non-negative number");
            }

            var notifications = await _context.Notifications
                    .Where(n => n.ReceiverId.Equals(userId) && n.TopicId != null)
                    .OrderByDescending(n => n.CreatedAt)
                    .Include(n => n.Sender)
                    .Include(n => n.Topic)
                    .Include(n => n.Comment)
                    .ToListAsync();

            var output = new List<NotificationDto>();

            foreach (var notification in notifications.GroupBy(c => new { c.Type, c.TopicId, c.CommentId }).Select(n => n.ToList()))
            {
                var first = notification.First();
                if (first.Comment == null && first.CommentId != null || first.Topic.IsDeleted)
                {
                    continue;
                }

                var count = notification.GroupBy(n => n.SenderId).Select(n => n.FirstOrDefault()).Count();
                output.Add(NotificationDto.Create(first, count - 1));
            }

            return limit == null ? output.Where(n => !n.IsRead) : output.Skip((int) limit).Take(5);
        }

        public async Task MarkAsRead(string userId, int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);

            if (notification == null || !notification.ReceiverId.Equals(userId))
            {
                throw new KeyNotFoundException("Notification not found");
            }

            notification.IsRead = true;

            await Do(() => _context.Entry(notification).State = EntityState.Modified);
        }

        public async Task NotifyVote(ApplicationUser sender, Topic topic, bool isUp)
        {
            var notification = new Notification
            {
                Type = isUp ? NotificationType.UpVote : NotificationType.DownVote,
                SenderId = sender.Id,
                ReceiverId = topic.UserId,
                TopicId = topic.Id
            };

            var result = await Has(notification, isUp ? NotificationType.DownVote : NotificationType.UpVote);

            if (result != null)
            {
                return;
            }

            await Notify(notification, sender, topic);
        }

        public async Task NotifyComment(ApplicationUser sender, Topic topic, int commentId)
        {
            var notification = new Notification
            {
                Type = NotificationType.Comment,
                SenderId = sender.Id,
                ReceiverId = topic.UserId,
                TopicId = topic.Id,
                NewId = commentId
            };

            await Notify(notification, sender, topic);
        }

        public async Task NotifyCommentVote(ApplicationUser sender, Comment comment, bool isUp)
        {
            var notification = new Notification
            {
                Type = isUp ? NotificationType.CommentUpVote : NotificationType.CommentDownVote,
                SenderId = sender.Id,
                ReceiverId = comment.UserId,
                TopicId = comment.TopicId,
                CommentId = comment.Id
            };

            var result = await Has(notification, isUp ? NotificationType.CommentDownVote : NotificationType.CommentUpVote);

            if (result != null)
            {
                return;
            }

            await Notify(notification, sender, comment.Topic, comment);
        }

        public async Task NotifyReply(ApplicationUser sender, Comment comment, int replyId)
        {
            var notification = new Notification
            {
                Type = NotificationType.Reply,
                SenderId = sender.Id,
                ReceiverId = comment.UserId,
                TopicId = comment.TopicId,
                CommentId = comment.Id,
                NewId = replyId
            };

            await Notify(notification, sender, comment.Topic, comment);
        }

        private async Task Notify(Notification notification, ApplicationUser sender, Topic topic, Comment comment = null)
        {
            try
            {
                await Do(async () => await _context.Notifications.AddAsync(notification));
                _notificationsHub.Notify(notification.ReceiverId, NotificationDto.Create(notification, sender, topic, comment));
            }
            catch
            {
                // ignored
            }
        }

        private async Task<Notification> Has(Notification notification, NotificationType? extra = null)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => (n.Type == notification.Type || n.Type == extra) &&
                                          n.SenderId.Equals(notification.SenderId) &&
                                          n.TopicId == notification.TopicId &&
                                          n.CommentId == notification.CommentId);
        }

        //private async Task<bool> UpdateOnHas(Notification notification, string uri = null, string firstName = null, NotificationType? extra = null, bool renewable = true)
        //{
        //    var exist = await Has(notification, extra);

        //    if (exist != null)
        //    {
        //        if (!renewable && exist.SenderId.Equals(notification.SenderId))
        //        {
        //            return true;
        //        }

        //        exist.IsRead = false;
        //        exist.SenderId = notification.SenderId;

        //        await Update(db => db.Entry(exist).State = EntityState.Modified);

        //        if (renewable)
        //        {
        //            _notificationsHub.Notify(notification.ReceiverId, NotificationDto.Create(notification, uri, firstName));
        //        }
        //    }

        //    return exist != null;
        //}

        //private async Task Update(Action<ApplicationDbContext> action)
        //{
        //    using (var db = new ApplicationDbContext(_options))
        //    {
        //        action(db);
        //        await db.SaveChangesAsync();
        //    }
        //}
    }
}
