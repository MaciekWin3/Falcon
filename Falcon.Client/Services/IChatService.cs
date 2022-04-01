namespace Falcon.Client.Services
{
    public interface IChatService
    {
        void ExecuteCommand();

        Task RunAsync();
    }
}