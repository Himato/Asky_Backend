using System.Threading.Tasks;
using Asky.Helpers;
using Asky.Models;
using Asky.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asky.Controllers
{
    [Authorize(Policy = nameof(UserRole.Admin))]
    public class AdminController : ApiHelper
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Route("Users")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetUsers()
        {
            return await Do(async () => await _userService.GetUsers());
        }
    }
}