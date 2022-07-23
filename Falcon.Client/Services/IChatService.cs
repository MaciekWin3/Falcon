namespace Falcon.Client.Services
{
    public interface IChatService
    {
        Task RunAsync();

        Task RunChat();
    }
}