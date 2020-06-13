using System.Threading.Tasks;
using Asky.Dtos;
using Asky.Services;
using Microsoft.AspNetCore.SignalR;

namespace Asky.Hubs
{
    public class TopicsHub : Hub
    {
        private readonly ITopicService _topicService;

        public TopicsHub(ITopicService topicService)
        {
            _topicService = topicService;
        }

        public async Task Connect(int topicId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, topicId.ToString());

            try
            {
                var userId = Context.User.Identity.IsAuthenticated ? Context.User.Identity.GetUserId() : null;

                await _topicService.AddView(userId, topicId);
            }
            catch
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, topicId.ToString());
            }
        }
    }

    public static class TopicsHubExtensions
    {
        public static async void SendComment(this IHubContext<TopicsHub> hub, int topicId, ViewCommentDto comment)
        {
            await hub.Clients.Group(topicId.ToString()).SendAsync("ReceiveComment", comment);
        }

        public static async void SendReply(this IHubContext<TopicsHub> hub, int topicId, ViewReplyDto reply)
        {
            await hub.Clients.Group(topicId.ToString()).SendAsync("ReceiveReply", reply);
        }
    }
}
