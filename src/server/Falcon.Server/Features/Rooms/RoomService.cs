using Falcon.Server.Features.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Falcon.Server.Features.Rooms
{
    public class RoomService
    {
        private readonly IHubContext<ChatHub> chatHubContext;

        public RoomService(IHubContext<ChatHub> chatHubContext)
        {
            this.chatHubContext = chatHubContext;
        }
    }
}