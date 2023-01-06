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
    public class ChatHub : Hub<IChatHubClient>
    {
        private readonly string falconBot;
        private readonly IMessageService messageService;
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private IDictionary<string, UserConnection> Connections { get; set; }
        private HashSet<string> Rooms { get; set; }

        public ChatHub(IMessageService messageService, IConfiguration configuration,
            ILogger logger, IDictionary<string, UserConnection> connections, HashSet<string> rooms)
        {
            falconBot = "Falcon Bot";
            this.messageService = messageService;
            this.configuration = configuration;
            this.logger = logger;
            Connections = connections;
            Rooms = rooms;
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
                Clients.Group(userConnection.Room).ReceiveMessage(falconBot, $"{userConnection.Username} has left");
            }
            logger.Information("Connection closed: {0}, user: {1}", Context.ConnectionId, Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendGroupMessageAsync(string message)
        {
            if (Connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                await Clients.OthersInGroup(userConnection.Room).ReceiveMessage(userConnection.Username, message);
            }
            string encryptedAndCompressedMessage = EncryptMessage(message);
            //await Clients.Others.ReceiveMessage(userConnection.Username, message);
            await messageService.CreateAsync(new Message { Content = encryptedAndCompressedMessage });
        }

        public async Task SendDirectMessage(string message, string recipient)
        {
            // This needs fix, also for blazor client
            Connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection);
            var user = Connections.Values
               .Where(c => c.Username == recipient)
               .Select(c => c.Username)
               .FirstOrDefault();

            if (user is null)
            {
                await Clients.Caller.ReceiveMessage(falconBot, "User not found!");
            }
            else
            {
                // Needs fix
                await Clients.User(recipient).ReceiveMessage(userConnection.Username, $"[yellow]DM from {userConnection.Username}:{message}[/]");
                string encryptedAndCompressedMessage = EncryptMessage(message);
                await messageService.CreateAsync(new Message { Content = encryptedAndCompressedMessage });
            }
        }

        public async Task JoinRoomAsync(string room)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            Connections[Context.ConnectionId] = new UserConnection(Context.UserIdentifier, room);
            // Check if this is working
            await Clients.Group(room).Connected(Context.UserIdentifier);
            await Clients.Group(room).ReceiveMessage(falconBot,
                $"{Context.UserIdentifier} has joined {room}");
            logger.Information("User: {0}, with Id: {1} joined room {2}", Context.UserIdentifier, Context.ConnectionId, room);
        }

        public async Task QuitRoomAsync()
        {
            Connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userConnection.Room);
            Connections[Context.ConnectionId] = new UserConnection(Context.UserIdentifier, null);
            await Clients.Group(userConnection.Room).ReceiveMessage(falconBot,
                $"{Context.UserIdentifier} has left {userConnection.Room}");
            await Clients.Group(userConnection.Room).Disconected(Context.UserIdentifier);
            logger.Information("User: {0}, with Id: {1} left room {2}", Context.UserIdentifier, Context.ConnectionId, userConnection.Room);
        }

        public List<string> ShowActiveRooms()
        {
            return Rooms.ToList();
        }

        public List<string> ShowUsersInRoom()
        {
            string room = GetUserGroup();
            var users = Connections.Values
               .Where(c => c.Room == room)
               .Select(c => c.Username)
               .ToList();

            return users;
        }

        public bool CreateRoom(string room)
        {
            if (Rooms.Contains(room))
            {
                return false;
            }
            Rooms.Add(room);
            logger.Information("Room {0} created", room);
            return true;
        }

        private string GetUserGroup()
        {
            Connections.TryGetValue(Context.ConnectionId, out UserConnection userConnection);
            return userConnection.Room;
        }

        private string EncryptMessage(string message)
        {
            message = Cryptography.EncryptDecrypt(message, int.Parse(configuration["Encryption:Key"]));
            return message;
        }
    }
}