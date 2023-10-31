using Falcon.Client.Features.Auth.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Falcon.Client.Features.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<string> LoginAsync(User user)
        {
            try
            {
                var response = await AuthorizeAsync(user);
                if (!string.IsNullOrEmpty(response))
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

        private async Task<string> AuthorizeAsync(User user)
        {
            var httpClient = httpClientFactory.CreateClient("Server");
            var response = await httpClient.PostAsJsonAsync($"api/auth/login", user);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }
            var token = await GetTokenValue(response);
            return token;
        }

        private async Task<string> GetTokenValue(HttpResponseMessage loginResponse)
        {
            var jsonString = await loginResponse.Content.ReadAsStringAsync();
            var token = JsonSerializer.Deserialize<Token>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return token.Value;
        }
    }
}