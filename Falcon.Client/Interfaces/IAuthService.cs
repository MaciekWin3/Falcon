namespace Falcon.Client.Interfaces
{
    public interface IAuthService
    {
        Task<string> Login();
    }
}