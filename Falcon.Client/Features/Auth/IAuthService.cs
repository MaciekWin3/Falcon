namespace Falcon.Client.Features.Auth
{
    public interface IAuthService
    {
        Task<string> Login();
    }
}