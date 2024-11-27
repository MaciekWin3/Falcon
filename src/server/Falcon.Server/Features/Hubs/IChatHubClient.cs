namespace Falcon.Server.Hubs
{
    public interface IChatHubClient
    {
        Task ReceiveMessage(string user, string message);

        Task OnConnected();

        Task OnDisconnected();
    }
}