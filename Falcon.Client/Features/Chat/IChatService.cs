namespace Falcon.Client.Features.Chat
{
    public interface IChatService
    {
        Task RunAsync(string token);

        Task<string> GetUsernameAsync();
    }
}