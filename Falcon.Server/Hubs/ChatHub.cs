using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Features.Messages.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Falcon.Server.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IMessageService messageService;
        private readonly IUserIdProvider userIdProvider;

        public ChatHub(IMessageService messageService, IUserIdProvider userIdProvider)
        {
            _botUser = "MyChat Bot";
            this.messageService = messageService;
            this.userIdProvider = userIdProvider;
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"-----> Connection established: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser,
                $"{userConnection.User} has joined {userConnection.Room}");
        }

        public async Task SendMessageAsync(string userName, string message)
        {
            // Need some changes
            var user = Context.User.Identities.FirstOrDefault().Claims.FirstOrDefault().Value;
            await Clients.Others.SendAsync("ReceiveMessage", user, message);
            await messageService.CreateAsync(new Message { Content = message });
        }
    }
}