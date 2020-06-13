using System.Threading.Tasks;
using Asky.Helpers;
using Asky.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Asky.Controllers
{
    [Authorize]
    public class UsersController : ApiHelper
    {
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;

        public UsersController(IUserService userService, INotificationService notificationService)
        {
            _userService = userService;
            _notificationService = notificationService;
        }

        [HttpPut]
        [Route("Notifications")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            return await Do(async () => await _notificationService.MarkAsRead(User.Identity.GetUserId(), notificationId));
        }

        [HttpGet]
        [Route("Notifications")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNotifications(int? limit)
        {
            return await Do(async () => await _notificationService.GetNotifications(User.Identity.GetUserId(), limit));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfileView(string username)
        {
            return await Do(async () => await _userService.GetProfileView(username));
        }

        [HttpGet]
        [Route("Bookmarks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyBookmarks()
        {
            return await Do(async () => await _userService.GetMyBookmarks(User.Identity.GetUserId()));
        }

        [HttpGet]
        [Route("UpVotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyUpVotes()
        {
            return await Do(async () => await _userService.GetMyUpVotes(User.Identity.GetUserId()));
        }

        [HttpGet]
        [Route("DownVotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyDownVotes()
        {
            return await Do(async () => await _userService.GetMyDownVotes(User.Identity.GetUserId()));
        }

        [HttpGet]
        [Route("History")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHistory()
        {
            return await Do(async () => await _userService.GetHistory(User.Identity.GetUserId()));
        }
    }
}