namespace Falcon.Client.Features.SignalR
{
    public interface ISignalRClient
    {
        event Action<string, string> OnReceiveMessage;

        Task StartConnectionAsync(string token);
    }
}