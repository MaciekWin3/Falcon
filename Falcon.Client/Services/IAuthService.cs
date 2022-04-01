namespace Falcon.Client.Services
{
    public interface IAuthService
    {
        Task<string> Login();
    }
}