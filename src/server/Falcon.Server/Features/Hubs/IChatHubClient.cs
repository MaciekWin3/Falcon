namespace Falcon.Server.Hubs
{
    public interface IChatHubClient
    {
        Task ReceiveMessage(string user, string message);

        Task OnConnected(string username);

        Task OnDisconnected(string username);
    }
}