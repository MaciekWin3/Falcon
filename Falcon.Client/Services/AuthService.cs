using Falcon.Client.DTOs;
using Falcon.Client.Windows;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;

namespace Falcon.Client.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private readonly ChatService chatService;

        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ChatService chatService)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.chatService = chatService;
        }

        public LoginWindow CreateLoginWindow()
        {
            return new LoginWindow();
        }

        public async Task<string> Login(string username, string password)
        {
            var userCredentials = new UserDTO()
            {
                Username = username,
                Password = password
            };
            try
            {
                var response = await Authorize(userCredentials);
                if (response.Length != 0)
                {
                    return response;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private async Task<string> Authorize(UserDTO userDTO)
        {
            var httpClient = httpClientFactory.CreateClient("Server");
            var response = await httpClient.PostAsJsonAsync($"api/auth/login", userDTO);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var tokenDTO = JsonConvert.DeserializeObject<TokenDTO>(jsonString);
            var token = tokenDTO.Token;
            return token;
        }
    }
}