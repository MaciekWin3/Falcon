using Falcon.Client.DTOs;
using Newtonsoft.Json;
using Spectre.Console;
using System.Net.Http.Json;

namespace Falcon.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private static readonly string baseUrl = "https://localhost:7262";

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        // Add edge cases
        public async Task<string> Login()
        {
            for (int i = 0; i < 3; i++)
            {
                var userCredentials = GetUserCredentials();

                var response = await Authorize(userCredentials);
                if (response.Length != 0) // Maybe there is a better way
                {
                    AnsiConsole.MarkupLine($"[green]Login successful![/]");
                    return response;
                }
                else
                {
                    ShowErrorMessage(i);
                }
            }
            return string.Empty;
        }

        private UserDTO GetUserCredentials()
        {
            var username = AnsiConsole.Ask<string>("Username: ");
            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("Password: ")
                    .Secret());

            var userDTO = new UserDTO()
            {
                Username = username,
                Password = password
            };

            return userDTO;
        }

        private async Task<string> Authorize(UserDTO userDTO)
        {
            var httpClient = httpClientFactory.CreateClient();
            var response = await httpClient.PostAsJsonAsync($"{baseUrl}/api/auth/login", userDTO);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }
            var jsonString = await response.Content.ReadAsStringAsync();
            var tokenDTO = JsonConvert.DeserializeObject<TokenDTO>(jsonString);
            var token = tokenDTO.Token;
            return token;
        }

        private void ShowErrorMessage(int tryNumber)
        {
            if (tryNumber == 2)
            {
                AnsiConsole.MarkupLine($"[red]You entered wrong credentials three times. Application will quit![/]");
                Environment.Exit(-1);
            }
            else
            {
                AnsiConsole.MarkupLine($"[magenta]Wrong credentials! Try again![/]");
            }
        }
    }
}