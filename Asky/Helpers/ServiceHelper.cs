using System;
using System.Threading.Tasks;
using Asky.Models;

namespace Asky.Helpers
{
    public class ServiceHelper
    {
        private readonly ApplicationDbContext _context;

        public ServiceHelper(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Do(Func<Task> func)
        {
            await func();

            var result = await _context.SaveChangesAsync();

            if (result == 0)
            {
                throw new ArgumentException("Failed to save changes");
            }
        }

        public async Task Do(Action action)
        {
            action();

            var result = await _context.SaveChangesAsync();

            if (result == 0)
            {
                throw new ArgumentException("Failed to save changes");
            }
        }
    }
}
