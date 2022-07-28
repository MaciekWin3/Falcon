namespace Falcon.Client.Interfaces
{
    public interface IChatService
    {
        Task RunAsync();

        Task RunChat();
    }
}