using Falcon.Client.Features.Auth;
using Falcon.Client.Features.Auth.UI;
using Falcon.Client.Features.Chat;
using Falcon.Client.Features.Chat.UI;
using Falcon.Client.Features.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace Falcon.Client
{
    public sealed class TerminalOrchestrator
    {
        private readonly ChatService chatService;
        private readonly AuthService authService;
        private readonly SignalRClient signalRClient;
        private readonly IServiceProvider serviceProvider;
        private static Dictionary<string, string> parameters { get; set; }

        public TerminalOrchestrator(ChatService chatService, AuthService authService,
            SignalRClient signalRClient, IServiceProvider serviceProvider)
        {
            parameters = new Dictionary<string, string>();
            this.chatService = chatService;
            this.authService = authService;
            this.signalRClient = signalRClient;
            this.serviceProvider = serviceProvider;
        }

        public void InitApp()
        {
            Application.Init();
            Application.Run(CreateLoginWindow());
            Console.OutputEncoding = System.Text.Encoding.Default;
            Application.Shutdown();
        }

        public void ChangeWindow(Window window, MenuBar menuBar = null)
        {
            Application.RequestStop();
            Toplevel appWindow = new()
            {
                Title = "Falcon"
            };
            appWindow.Add(window);

            if (menuBar is not null)
            {
                appWindow.Add(menuBar);
            }
            Application.Run(appWindow);
        }

        private LoginWindow CreateLoginWindow()
        {
            var loginWindow = serviceProvider.GetService<LoginWindow>();
            loginWindow.OnAuthorize = authService.LoginAsync;
            loginWindow.OnLogin = async (token) =>
            {
                await signalRClient.StartConnectionAsync(token);
                Application.Invoke(async () =>
                {
                    var room = "All";
                    await signalRClient.connection.InvokeCoreAsync("JoinRoomAsync", args: [room]);
                    var chatWindow = CreateChatWindow();
                    var menuBar = chatWindow.CreateMenuBar();
                    ChangeWindow(chatWindow, menuBar);
                });
            };

            return loginWindow;
        }

        private ChatWindow CreateChatWindow()
        {
            var chatWindow = serviceProvider.GetService<ChatWindow>();
            return chatWindow;
        }
    }
}