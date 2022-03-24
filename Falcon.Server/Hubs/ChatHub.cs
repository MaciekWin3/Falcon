using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Features.Messages.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Falcon.Server.Hubs
{
    [Authorize()]
    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IMessageService messageService;

        public ChatHub(IMessageService messageService)
        {
            _botUser = "MyChat Bot";
            this.messageService = messageService;
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
            await Clients.Others.SendAsync("ReceiveMessage", userName, message);
            await messageService.CreateAsync(new Message { Content = message });
        }
    }
}