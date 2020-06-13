using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asky.Dtos;
using Asky.Helpers;
using Asky.Hubs;
using Asky.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Asky.Services
{
    public interface ICommentService
    {
        Task<ViewCommentDto> AddComment(string userId, int topicId, CommentDto commentDto);

        Task UpdateComment(string userId, int commentId, CommentDto commentDto);

        Task DeleteComment(string userId, int commentId);

        Task Vote(string userId, int commentId, bool isUp);

        Task<ViewReplyDto> AddReply(string userId, int commentId, ReplyDto replyDto);

        Task UpdateReply(string userId, int replyId, ReplyDto replyDto);

        Task DeleteReply(string userId, int replyId);
    }

    public class CommentsService : ServiceHelper, ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly ITopicService _topicService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<TopicsHub> _topicsHub;

        public CommentsService(ApplicationDbContext context, IUserService userService,
            ITopicService topicService, INotificationService notificationService, IHubContext<TopicsHub> topicsHub) : base(context)
        {
            _context = context;
            _userService = userService;
            _topicService = topicService;
            _notificationService = notificationService;
            _topicsHub = topicsHub;
        }

        public async Task<ViewCommentDto> AddComment(string userId, int topicId, CommentDto commentDto)
        {
            var topic = await _topicService.GetTopic(topicId);

            var comment = new Comment
            {
                Content = commentDto.Content,
                UserId = userId,
                TopicId = topicId
            };

            await Do(async () => await _context.Comments.AddAsync(comment));

            comment.User = await _userService.GetUserById(userId);

            if (userId != topic.UserId)
            {
                await _notificationService.NotifyComment(comment.User, topic, comment.Id);
            }

            var dto = ViewCommentDto.Create(userId, comment);

            _topicsHub.SendComment(topic.Id, dto);

            return dto;
        }

        public async Task UpdateComment(string userId, int commentId, CommentDto commentDto)
        {
            var comment = await GetUserComment(userId, commentId);

            comment.Content = commentDto.Content;

            await Do(() => _context.Entry(comment).State = EntityState.Modified);
        }

        public async Task DeleteComment(string userId, int commentId)
        {
            var comment = await GetUserComment(userId, commentId);

            await Do(() => _context.Comments.Remove(comment));
        }

        public async Task Vote(string userId, int commentId, bool isUp)
        {
            var vote = await _context.CommentVotes
                .FirstOrDefaultAsync(v => v.UserId.Equals(userId) && v.CommentId == commentId);

            if (vote != null)
            {
                if (vote.IsUp == isUp)
                {
                    await Do(() => _context.CommentVotes.Remove(vote));
                }
                else
                {
                    vote.IsUp = isUp;
                    await Do(() => _context.Entry(vote).State = EntityState.Modified);
                }

                return;
            }

            var comment = await GetComment(commentId);

            vote = new CommentVote
            {
                IsUp = isUp,
                CommentId = commentId,
                UserId = userId
            };

            await Do(async () => await _context.CommentVotes.AddAsync(vote));

            var sender = await _userService.GetUserById(userId);

            if (userId != comment.UserId)
            {
                await _notificationService.NotifyCommentVote(sender, comment, isUp);
            }
        }

        public async Task<ViewReplyDto> AddReply(string userId, int commentId, ReplyDto replyDto)
        {
            var comment = await GetComment(commentId);

            var reply = new Reply
            {
                Content = replyDto.Content,
                CommentId = commentId,
                UserId = userId
            };

            await Do(async () => await _context.Replies.AddAsync(reply));

            reply.User = await _userService.GetUserById(userId);

            if (userId != comment.UserId)
            {
                await _notificationService.NotifyReply(reply.User, comment, reply.Id);
            }

            var dto = ViewReplyDto.Create(reply);

            _topicsHub.SendReply(comment.TopicId, dto);

            return dto;
        }

        public async Task UpdateReply(string userId, int replyId, ReplyDto replyDto)
        {
            var reply = await GetUserReply(userId, replyId);

            reply.Content = replyDto.Content;

            await Do(() => _context.Entry(reply).State = EntityState.Modified);
        }

        public async Task DeleteReply(string userId, int replyId)
        {
            var reply = await GetUserReply(userId, replyId);

            await Do(() => _context.Replies.Remove(reply));
        }

        private async Task<Comment> GetUserComment(string userId, int commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.Topic)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.UserId.Equals(userId) && !c.Topic.IsDeleted);

            if (comment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            return comment;
        }

        private async Task<Comment> GetComment(int commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.Topic)
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.Topic.IsDeleted);

            if (comment == null)
            {
                throw new ArgumentException("Comment doesn't exist, or it may have been deleted");
            }

            return comment;
        }

        private async Task<Reply> GetUserReply(string userId, int replyId)
        {
            var reply = await _context.Replies.FindAsync(replyId);

            if (reply == null || !reply.UserId.Equals(userId))
            {
                throw new ArgumentException("Reply not found");
            }

            return reply;
        }
    }
}
