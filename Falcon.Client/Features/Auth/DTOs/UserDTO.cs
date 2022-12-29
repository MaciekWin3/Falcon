namespace Falcon.Client.Features.Auth.DTOs
{
    public record UserDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}