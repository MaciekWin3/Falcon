namespace Falcon.Client.Interfaces.SignalRClients
{
    public interface IChatHubClient
    {
        Task ReceiveMessage(string user, string message);
    }
}