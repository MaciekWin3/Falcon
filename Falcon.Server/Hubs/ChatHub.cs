using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Features.Messages.Services;
using Falcon.Server.Utils;
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
        private readonly IConfiguration configuration;
        private readonly IDictionary<string, UserConnection> connections;

        public ChatHub(IMessageService messageService, IConfiguration configuration,
            IDictionary<string, UserConnection> connections)
        {
            _botUser = "MyChat Bot";
            this.messageService = messageService;
            this.configuration = configuration;
            this.connections = connections;
        }

        // Connections management
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"-----> Connection established: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");
                SendUsersConnected(userConnection.Room);
            }
            Console.WriteLine($"-----> Connection closed: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("Send", $"{Context.ConnectionId} has left the group {groupName}.");
        }

        public async Task JoinRoom(UserConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
            connections[Context.ConnectionId] = userConnection;
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser,
                $"{userConnection.User} has joined {userConnection.Room}");
            await SendUsersConnected(userConnection.Room);
        }

        public async Task SendMessageAsync(string message)
        {
            // Need some changes
            var user = Context.UserIdentifier;
            if (connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, message);
            }
            string encryptedAndCompressedMessage = CompressAndEncryptMessage(message);
            await Clients.Others.SendAsync("ReceiveMessage", user, message);
            await messageService.CreateAsync(new Message { Content = encryptedAndCompressedMessage });
        }

        public Task SendUsersConnected(string room)
        {
            var users = connections.Values
                .Where(c => c.Room == room)
                .Select(c => c.User);

            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }

        public Task SendActiveRooms()
        {
            var rooms = connections.Values
                .Select(c => c.Room)
                .ToList();

            return Clients.All.SendAsync("ActiveRooms", rooms);
        }

        private string CompressAndEncryptMessage(string message)
        {
            message = StringCompression.Compress(message);
            message = Cryptography.EncryptDecrypt(message, int.Parse(configuration["Encryption:Key"]));
            return message;
        }
    }
}