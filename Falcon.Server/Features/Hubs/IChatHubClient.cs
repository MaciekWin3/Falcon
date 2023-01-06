namespace Falcon.Server.Hubs
{
    public interface IChatHubClient
    {
        Task ReceiveMessage(string user, string message);

        Task Connected(string username);

        Task Disconected(string username);
    }
}