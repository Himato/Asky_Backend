using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Asky.Helpers.Attributes.Searchable;
using Asky.Models;

namespace Asky.Dtos
{
    public class TopicDto
    {
        [Required]
        [MinLength(16, ErrorMessage = "Title can't be less than 16 characters long")]
        [MaxLength(128, ErrorMessage = "Title can't be more than 128 characters long")]
        public string Title { get; set; }

        [Required]
        [MaxLength(10000, ErrorMessage = "Content can't be more than 10000 characters long")]
        public string Content { get; set; }

        public int CategoryId { get; set; }
    }

    public class UpdateTopicDto
    {
        [Required]
        [MaxLength(10000, ErrorMessage = "Content can't be more than 10000 characters long")]
        public string Content { get; set; }

        public int CategoryId { get; set; }
    }

    public class UserTopicDto
    {
        public int Id { get; set; }

        public string Uri { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int CategoryId { get; set; }

        public static UserTopicDto Create(Topic topic)
        {
            return new UserTopicDto
            {
                Id = topic.Id,
                Uri = topic.Uri,
                Title = topic.Title,
                Content = topic.Content,
                CategoryId = topic.CategoryId
            };
        }
    }

    public class CommentDto
    {
        [Required]
        public string Content { get; set; }
    }

    public class ReplyDto
    {
        [Required]
        public string Content { get; set; }
    }

    public class TopicResultDto
    {
        public string Uri { get; set; }

        [SearchableString]
        public string Title { get; set; }

        public ViewCategoryDto Category { get; set; }

        public ViewUserDto User { get; set; }

        public int NumberOfComments { get; set; }

        public int NumberOfViews { get; set; }

        public DateTime CreatedAt { get; set; }

        public static TopicResultDto Create(Topic topic)
        {
            return new TopicResultDto
            {
                Uri = topic.Uri,
                Title = topic.Title,
                Category = ViewCategoryDto.Create(topic.Category),
                User = ViewUserDto.Create(topic.User),
                NumberOfComments = topic.Comments.Count,
                NumberOfViews = topic.Views.Count,
                CreatedAt = DateTime.SpecifyKind(topic.CreatedAt, DateTimeKind.Utc)
            };
        }
    }

    public class ViewTopicDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Uri { get; set; }

        public ViewCategoryDto Category { get; set; }

        public ViewUserDto User { get; set; }

        public DateTime CreatedAt { get; set; }

        public int NumberOfUpVotes { get; set; }

        public int NumberOfDownVotes { get; set; }

        public bool? IsUpVoted { get; set; }

        public bool IsBookmarked { get; set; }

        public IEnumerable<ViewCommentDto> Comments { get; set; }

        public static ViewTopicDto Create(string userId, Topic topic, bool isBookmarked)
        {
            return new ViewTopicDto
            {
                Id = topic.Id,
                Title = topic.Title,
                Content = topic.Content,
                Uri = topic.Uri,
                Category = ViewCategoryDto.Create(topic.Category),
                User = ViewUserDto.Create(topic.User),
                CreatedAt = topic.CreatedAt,
                NumberOfUpVotes = topic.Votes.Count(c => c.IsUp),
                NumberOfDownVotes = topic.Votes.Count(c => !c.IsUp),
                IsUpVoted = userId != null ? topic.Votes.FirstOrDefault(c => c.UserId.Equals(userId))?.IsUp : null,
                IsBookmarked = isBookmarked,
                Comments = topic.Comments.Select(c => ViewCommentDto.Create(userId, c))
            };
        }
    }

    public class ViewCommentDto
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public ViewUserDto User { get; set; }

        public DateTime CreatedAt { get; set; }

        public int NumberOfUpVotes { get; set; }

        public int NumberOfDownVotes { get; set; }

        public bool? IsUpVoted { get; set; }

        public IEnumerable<ViewReplyDto> Replies { get; set; }

        public static ViewCommentDto Create(string userId, Comment comment)
        {
            return new ViewCommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                User = ViewUserDto.Create(comment.User),
                CreatedAt = comment.CreatedAt,
                NumberOfUpVotes = comment.Votes.Count(c => c.IsUp),
                NumberOfDownVotes = comment.Votes.Count(c => !c.IsUp),
                IsUpVoted = userId != null ? comment.Votes.FirstOrDefault(c => c.UserId.Equals(userId))?.IsUp : null,
                Replies = comment.Replies.Select(ViewReplyDto.Create)
            };
        }
    }

    public class ViewReplyDto
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public ViewUserDto User { get; set; }

        public int CommentId { get; set; }

        public static ViewReplyDto Create(Reply reply)
        {
            return new ViewReplyDto
            {
                Id = reply.Id,
                Content = reply.Content,
                CreatedAt = reply.CreatedAt,
                User = ViewUserDto.Create(reply.User),
                CommentId = reply.CommentId
            };
        }
    }
}
