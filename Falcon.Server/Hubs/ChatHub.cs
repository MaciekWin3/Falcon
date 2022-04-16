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
        private IDictionary<string, UserConnection> connections { get; set; }

        private HashSet<string> Rooms { get; set; } = new HashSet<string>()
        {
            "All",
            "Programming"
        };

        public ChatHub(IMessageService messageService, IConfiguration configuration,
            IDictionary<string, UserConnection> connections)
        {
            _botUser = "Falcon Bot";
            this.messageService = messageService;
            this.configuration = configuration;
            this.connections = connections;
        }

        // Connections management
        public override Task OnConnectedAsync()
        {
            var user = Context.UserIdentifier;
            connections.Add(Context.ConnectionId, new UserConnection(user, "All"));
            Console.WriteLine($"-----> Connection established: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.Username} has left");
                SendUsersConnected(userConnection.Room);
            }
            Console.WriteLine($"-----> Connection closed: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendGroupMessageAsync(string message)
        {
            // Need some changes
            var user = Context.UserIdentifier;
            if (connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.OthersInGroup(userConnection.Room).SendAsync("ReceiveMessage", userConnection.Username, message);
            }
            string encryptedAndCompressedMessage = CompressAndEncryptMessage(message);
            //await Clients.Others.SendAsync("ReceiveMessage", user, message);
            var x = Context.Items;
            await messageService.CreateAsync(new Message { Content = encryptedAndCompressedMessage });
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

        public async Task JoinRoom(string room)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            connections[Context.ConnectionId] = new UserConnection(Context.UserIdentifier, room);
            await Clients.Group(room).SendAsync("ReceiveMessage", _botUser,
                $"{Context.UserIdentifier} has joined {room}");
            await SendUsersConnected(room);
        }

        public Task SendUsersConnected(string room)
        {
            var users = connections.Values
                .Where(c => c.Room == room)
                .Select(c => c.Username);

            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }

        public List<string> ShowActiveRooms()
        {
            return Rooms.ToList();
        }

        private string CompressAndEncryptMessage(string message)
        {
            message = StringCompression.Compress(message);
            message = Cryptography.EncryptDecrypt(message, int.Parse(configuration["Encryption:Key"]));
            return message;
        }
    }
}