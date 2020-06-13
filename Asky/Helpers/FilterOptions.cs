using System.Linq;
using Asky.Models;

namespace Asky.Helpers
{
    public class FilterOptions
    {
        public int? CategoryId { get; set; }

        public bool? Latest { get; set; }

        public bool? Unread { get; set; }

        public bool? Popular { get; set; }

        public bool? TopVoted { get; set; }

        public IQueryable<Topic> Apply(IQueryable<Topic> query, string userId)
        {
            if (CategoryId != null)
            {
                query = query.Where(t => t.CategoryId == CategoryId).AsQueryable();
            }

            if (Latest != null)
            {
                return query.OrderByDescending(t => t.CreatedAt).AsQueryable();
            }

            if (Unread != null && userId != null)
            {
                return query.Where(t => !t.Views.Select(c => c.UserId).Contains(userId)).OrderByDescending(t => t.CreatedAt).AsQueryable();
            }

            if (Popular != null)
            {
                return query
                    .OrderByDescending(t => t.Views.Count)
                    .ThenByDescending(t => t.Comments.Count)
                    .ThenByDescending(t => t.CreatedAt)
                    .AsQueryable();
            }

            if (TopVoted != null)
            {
                return query.OrderByDescending(t => t.Votes.Count(c => c.IsUp)).AsQueryable();
            }

            return query;
        }
    }
}
