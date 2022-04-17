using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Features.Messages.Services;
using Falcon.Server.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ILogger = Serilog.ILogger;

namespace Falcon.Server.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly string falconBot;
        private readonly IMessageService messageService;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private IDictionary<string, UserConnection> Connections { get; set; }

        private HashSet<string> Rooms { get; set; } = new HashSet<string>()
        {
            "All",
            "Programming"
        };

        public ChatHub(IMessageService messageService, IConfiguration configuration,
            ILogger logger, IDictionary<string, UserConnection> connections)
        {
            falconBot = "Falcon Bot";
            this.messageService = messageService;
            this.configuration = configuration;
            this.logger = logger;
            this.Connections = connections;
        }

        public override Task OnConnectedAsync()
        {
            var user = Context.UserIdentifier;
            Connections.Add(Context.ConnectionId, new UserConnection(user, "All"));
            logger.Information("Connection established: {0}, user: {1}", Context.ConnectionId, user);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (Connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                Connections.Remove(Context.ConnectionId);
                Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", falconBot, $"{userConnection.Username} has left");
                SendUsersConnected(userConnection.Room);
            }
            logger.Information("Connection closed: {0}, user: {1}", Context.ConnectionId, Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendGroupMessageAsync(string message)
        {
            if (Connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.OthersInGroup(userConnection.Room).SendAsync("ReceiveMessage", userConnection.Username, message);
            }
            string encryptedAndCompressedMessage = CompressAndEncryptMessage(message);
            //await Clients.Others.SendAsync("ReceiveMessage", user, message);
            await messageService.CreateAsync(new Message { Content = encryptedAndCompressedMessage });
        }

        public async Task JoinRoom(string room)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            Connections[Context.ConnectionId] = new UserConnection(Context.UserIdentifier, room);
            await Clients.Group(room).SendAsync("ReceiveMessage", falconBot,
                $"{Context.UserIdentifier} has joined {room}");
            logger.Information("User: {0}, with Id: {1} joined room {2}", Context.UserIdentifier, Context.ConnectionId, room);
            await SendUsersConnected(room);
        }

        public async Task QuitRoom()
        {
            Connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userConnection.Room);
            Connections[Context.ConnectionId] = new UserConnection(Context.UserIdentifier, null);
            await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", falconBot,
                $"{Context.UserIdentifier} has left {userConnection.Room}");
            logger.Information("User: {0}, with Id: {1} left room {2}", Context.UserIdentifier, Context.ConnectionId, userConnection.Room);
        }

        public Task SendUsersConnected(string room)
        {
            var users = Connections.Values
                .Where(c => c.Room == room)
                .Select(c => c.Username);

            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }

        public List<string> ShowActiveRooms()
        {
            return Rooms.ToList();
        }

        public List<string> ShowActiveUsers()
        {
        }

        private string CompressAndEncryptMessage(string message)
        {
            message = StringCompression.Compress(message);
            message = Cryptography.EncryptDecrypt(message, int.Parse(configuration["Encryption:Key"]));
            return message;
        }
    }
}