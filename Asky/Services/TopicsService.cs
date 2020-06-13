using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asky.Dtos;
using Asky.Helpers;
using Asky.Helpers.Attributes.Searchable;
using Asky.Models;
using Microsoft.EntityFrameworkCore;

namespace Asky.Services
{
    public interface ITopicService
    {
        Task<IEnumerable<TopicResultDto>> GetAll(string userId, SearchOptions<TopicResultDto, Topic> searchOptions,
            PagingOptions pagingOptions, FilterOptions filterOptions);

        Task<ViewTopicDto> GetViewTopic(string userId, string uri);

        Task<UserTopicDto> GetUserTopic(string userId, int id);

        Task<Topic> GetTopic(int id);

        Task<string> AddTopic(string userId, TopicDto topicDto);

        Task UpdateTopic(string userId, int topicId, UpdateTopicDto updateTopicDto);

        Task DeleteTopic(string userId, int topicId);

        Task Vote(string userId, int topicId, bool isUp);

        Task Bookmark(string userId, int topicId);

        Task AddView(string userId, int topicId);
    }

    public class TopicsService : ServiceHelper, ITopicService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public TopicsService(ApplicationDbContext context, IUserService userService, INotificationService notificationService) : base(context)
        {
            _context = context;
            _userService = userService;
            _notificationService = notificationService;
        }

        public async Task<IEnumerable<TopicResultDto>> GetAll(string userId, SearchOptions<TopicResultDto, Topic> searchOptions,
            PagingOptions pagingOptions, FilterOptions filterOptions)
        {
            var query = _context.Topics
                .Where(t => !t.IsDeleted)
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.Comments)
                .Include(t => t.Views)
                .AsQueryable();

            query = pagingOptions.Apply(filterOptions.Apply(searchOptions.Apply(query), userId));

            return (await query.ToListAsync()).Select(TopicResultDto.Create);
        }

        public async Task<ViewTopicDto> GetViewTopic(string userId, string uri)
        {
            var topic = await _context.Topics
                .Include(t => t.Category)
                .Include(t => t.User)
                .Include(t => t.Votes)
                .FirstOrDefaultAsync(t => t.Uri.Equals(uri) && !t.IsDeleted);

            if (topic == null)
            {
                throw new KeyNotFoundException("Topic not found");
            }

            var comments = await _context.Comments
                .Where(c => c.TopicId == topic.Id)
                .Include(c => c.User)
                .Include(c => c.Votes)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            foreach (var comment in comments)
            {
                comment.Replies = await _context.Replies
                    .Where(r => r.CommentId == comment.Id)
                    .Include(r => r.User)
                    .OrderBy(r => r.CreatedAt)
                    .ToListAsync();
            }

            topic.Comments = comments;

            var isBookmarked = userId != null && await _context.Bookmarks
                                   .AnyAsync(b => b.TopicId == topic.Id && b.UserId.Equals(userId));

            return ViewTopicDto.Create(userId, topic, isBookmarked);
        }

        public async Task<UserTopicDto> GetUserTopic(string userId, int id)
        {
            var topic = await _context.Topics.FindAsync(id);

            if (topic == null || !topic.UserId.Equals(userId) || topic.IsDeleted)
            {
                throw new KeyNotFoundException("Topic not found");
            }

            return UserTopicDto.Create(topic);
        }

        public async Task<Topic> GetTopic(int topicId)
        {
            var topic = await _context.Topics.FindAsync(topicId);

            if (topic == null || topic.IsDeleted)
            {
                throw new ArgumentException("Topic doesn't exist, or it may have been deleted by the author");
            }

            return topic;
        }

        public async Task<string> AddTopic(string userId, TopicDto topicDto)
        {
            var category = await _context.Categories.FindAsync(topicDto.CategoryId);

            if (category == null)
            {
                throw new ArgumentException("Invalid Category");
            }

            var topic = new Topic
            {
                Title = topicDto.Title,
                Content = topicDto.Content,
                Uri = topicDto.Title.GetUniqueUri(true),
                CategoryId = topicDto.CategoryId,
                UserId = userId
            };

            await Do(async () => await _context.Topics.AddAsync(topic));

            return topic.Uri;
        }

        public async Task UpdateTopic(string userId, int topicId, UpdateTopicDto updateTopicDto)
        {
            var topic = await _context.Topics.FindAsync(topicId);

            if (topic == null || !topic.UserId.Equals(userId) || topic.IsDeleted)
            {
                throw new KeyNotFoundException("Topic not found");
            }

            var category = await _context.Categories.FindAsync(updateTopicDto.CategoryId);

            if (category == null)
            {
                throw new ArgumentException("Invalid Category");
            }

            topic.Content = updateTopicDto.Content;
            topic.CategoryId = updateTopicDto.CategoryId;

            await Do(() => _context.Entry(topic).State = EntityState.Modified);
        }

        public async Task DeleteTopic(string userId, int topicId)
        {
            var topic = await _context.Topics.FindAsync(topicId);

            if (topic == null || !topic.UserId.Equals(userId) || topic.IsDeleted)
            {
                throw new KeyNotFoundException("Topic not found");
            }

            topic.IsDeleted = true;

            await Do(() => _context.Entry(topic).State = EntityState.Modified);
        }

        public async Task Vote(string userId, int topicId, bool isUp)
        {
            var vote = await _context.Votes
                    .FirstOrDefaultAsync(v => v.UserId.Equals(userId) && v.TopicId == topicId);

            if (vote != null)
            {
                if (vote.IsUp == isUp)
                {
                    await Do(() => _context.Votes.Remove(vote));
                }
                else
                {
                    vote.IsUp = isUp;
                    await Do(() => _context.Entry(vote).State = EntityState.Modified);
                }

                return;
            }

            var topic = await GetTopic(topicId);

            vote = new Vote
            {
                IsUp = isUp,
                TopicId = topicId,
                UserId = userId
            };

            await Do(async () => await _context.Votes.AddAsync(vote));

            var sender = await _userService.GetUserById(userId);

            if (userId != topic.UserId)
            {
                await _notificationService.NotifyVote(sender, topic, isUp);
            }
        }

        public async Task Bookmark(string userId, int topicId)
        {
            var bookmark =
                await _context.Bookmarks.FirstOrDefaultAsync(b => b.TopicId == topicId && b.UserId.Equals(userId));

            if (bookmark != null)
            {
                await Do(() => _context.Bookmarks.Remove(bookmark));
                return;
            }

            await GetTopic(topicId);

            bookmark = new Bookmark
            {
                TopicId = topicId,
                UserId = userId
            };

            await Do(async () => await _context.Bookmarks.AddAsync(bookmark));
        }

        public async Task AddView(string userId, int topicId)
        {
            await GetTopic(topicId);

            var view = new View
            {
                TopicId = topicId,
                UserId = userId
            };

            await _context.Views.AddAsync(view);
            await _context.SaveChangesAsync();
        }
    }
}
