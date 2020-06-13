using System.Threading.Tasks;
using Asky.Dtos;
using Asky.Helpers;
using Asky.Helpers.Attributes.Searchable;
using Asky.Models;
using Asky.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Asky.Controllers
{
    public class TopicsController : ApiHelper
    {
        private readonly ITopicService _topicService;
        private readonly ICommentService _commentService;

        public TopicsController(ITopicService topicService, ICommentService commentService)
        {
            _topicService = topicService;
            _commentService = commentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SearchOptions<TopicResultDto, Topic> searchOptions,
            [FromQuery] PagingOptions pagingOptions, [FromQuery] FilterOptions filterOptions)
        {
            var userId = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : null;

            return await Do(async () => await _topicService.GetAll(userId, searchOptions, pagingOptions, filterOptions));
        }

        [HttpGet]
        [Route("View")]
        public async Task<IActionResult> GetTopicView(string uri)
        {
            var userId = User.Identity.IsAuthenticated ? User.Identity.GetUserId() : null;

            return await Do(async () => await _topicService.GetViewTopic(userId, uri));
        }

        [HttpGet]
        [Authorize]
        [Route("User")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserTopic(int id)
        {
            return await Do(async () => await _topicService.GetUserTopic(User.Identity.GetUserId(), id));
        }

        [HttpPost]
        [Authorize]
        [Route("User")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddTopic(TopicDto topicDto)
        {
            return await Do(async () => await _topicService.AddTopic(User.Identity.GetUserId(), topicDto));
        }

        [HttpPut]
        [Authorize]
        [Route("User")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateTopic(int id, UpdateTopicDto updateTopicDto)
        {
            return await Do(async () => await _topicService.UpdateTopic(User.Identity.GetUserId(), id, updateTopicDto));
        }

        [HttpDelete]
        [Authorize]
        [Route("User")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            return await Do(async () => await _topicService.DeleteTopic(User.Identity.GetUserId(), id));
        }

        [HttpPut]
        [Authorize]
        [Route("VoteUp")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VoteUp(int id)
        {
            return await Do(async () => await _topicService.Vote(User.Identity.GetUserId(), id, true));
        }

        [HttpPut]
        [Authorize]
        [Route("VoteDown")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VoteDown(int id)
        {
            return await Do(async () => await _topicService.Vote(User.Identity.GetUserId(), id, false));
        }

        [HttpPut]
        [Authorize]
        [Route("Bookmark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Bookmark(int id)
        {
            return await Do(async () => await _topicService.Bookmark(User.Identity.GetUserId(), id));
        }

        [HttpPost]
        [Authorize]
        [Route("Comments")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddComments(int topicId, CommentDto commentDto)
        {
            return await Do(async () => await _commentService.AddComment(User.Identity.GetUserId(), topicId, commentDto));
        }

        [HttpPut]
        [Authorize]
        [Route("Comments")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateComment(int id, CommentDto commentDto)
        {
            return await Do(async () => await _commentService.UpdateComment(User.Identity.GetUserId(), id, commentDto));
        }

        [HttpDelete]
        [Authorize]
        [Route("Comments")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteComment(int id)
        {
            return await Do(async () => await _commentService.DeleteComment(User.Identity.GetUserId(), id));
        }

        [HttpPut]
        [Authorize]
        [Route("Comments/VoteUp")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CommentVoteUp(int id)
        {
            return await Do(async () => await _commentService.Vote(User.Identity.GetUserId(), id, true));
        }

        [HttpPut]
        [Authorize]
        [Route("Comments/VoteDown")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CommentVoteDown(int id)
        {
            return await Do(async () => await _commentService.Vote(User.Identity.GetUserId(), id, false));
        }

        [HttpPost]
        [Authorize]
        [Route("Comments/Replies")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddReply(int commentId, ReplyDto replyDto)
        {
            return await Do(async () => await _commentService.AddReply(User.Identity.GetUserId(), commentId, replyDto));
        }

        [HttpPut]
        [Authorize]
        [Route("Comments/Replies")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateReply(int id, ReplyDto replyDto)
        {
            return await Do(async () => await _commentService.UpdateReply(User.Identity.GetUserId(), id, replyDto));
        }

        [HttpDelete]
        [Authorize]
        [Route("Comments/Replies")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteReply(int id)
        {
            return await Do(async () => await _commentService.DeleteReply(User.Identity.GetUserId(), id));
        }
    }
}