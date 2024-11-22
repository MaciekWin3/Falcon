namespace Falcon.Client.Features.Chat
{
    public interface IChatHubClient
    {
        Task ReceiveMessage(string user, string message);
    }
}