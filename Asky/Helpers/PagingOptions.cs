using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Asky.Helpers
{
    public class PagingOptions
    {
        [Range(1, 99999, ErrorMessage = "Offset must be greater than 0.")]
        public int? Offset { get; set; }

        [Range(1, 100, ErrorMessage = "Limit must be greater than 0 and less than 100.")]
        public int? Limit { get; set; }

        public IQueryable<T> Apply<T>(IQueryable<T> query)
        {
            return query.Skip(Limit ?? 0).Take(Offset ?? 25);
        }
    }
}
