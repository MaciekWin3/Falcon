using Falcon.Client.DTOs;
using Falcon.Client.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Spectre.Console;
using System.Net.Http.Json;

namespace Falcon.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private static readonly string baseUrl = "https://localhost:7262";

        public AuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        // Add edge cases
        public async Task<string> Login()
        {
            for (int i = 0; i < 3; i++)
            {
                var userCredentials = GetUserCredentials();
                try
                {
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
                catch (Exception)

                {
                    AnsiConsole.MarkupLine($"[red]Something went wrong! Try again later![/]");
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        private static UserDTO GetUserCredentials()
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
            //var response = await httpClient.PostAsJsonAsync($"{configuration.GetValue<string>("IpAddress")}/api/auth/login", userDTO);
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

        private static void ShowErrorMessage(int tryNumber)
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