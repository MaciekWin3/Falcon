namespace Falcon.Client.Features.Chat
{
    public interface IChatService
    {
        Task RunAsync();

        Task RunChat();
    }
}