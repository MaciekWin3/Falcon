using Falcon.Client.Features.Auth;
using Falcon.Client.Features.Auth.UI;
using Falcon.Client.Features.Chat;
using Falcon.Client.Features.Chat.UI;
using Falcon.Client.Features.Lobby.UI;
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
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black);
            Console.OutputEncoding = System.Text.Encoding.Default;
            ShowLoginWindow();
            Application.Run();
            Application.Shutdown();
        }

        private void ShowLoginWindow()
        {
            Application.Top.RemoveAll();
            var top = Application.Top;
            var loginWindow = new LoginWindow
            {
                OnAuthorize = authService.LoginAsync,

                OnLogin = async (token) =>
                {
                    await signalRClient.StartConnectionAsync(token);
                    Application.MainLoop.Invoke(async () =>
                    {
                        await ShowRoomWindow();
                    });
                },

                OnExit = () =>
                {
                    Application.RequestStop();
                },

                OnQuit = () =>
                {
                    Application.RequestStop();
                },
            };
            top.Add(loginWindow);
            top.Add(loginWindow.CreateMenuBar());
            Application.Refresh();
        }

        private async Task ShowRoomWindow()
        {
            Application.Top.RemoveAll();
            var top = Application.Top;
            IList<string> rooms = new List<string>();
            rooms = await chatService.GetListOfRoomAsync();
            var win = new LobbyWindow(rooms)
            {
                OnChatOpen = async (room) =>
                {
                    if (room == "Create new room")
                    {
                        // TODO: Popup with creating new chat
                        throw new NotImplementedException();
                    }
                    await signalRClient.connection.InvokeCoreAsync("JoinRoomAsync", args: new[] { room });
                    ShowChatWindowNew();
                },

                OnQuit = () =>
                {
                    Application.RequestStop();
                }
            };
            top.Add(win);
            top.Add(win.CreateMenuBar());
            Application.Refresh();
        }

        private void ShowChatWindowNew()
        {
            Application.Top.RemoveAll();
            var top = Application.Top;
            var win = serviceProvider.GetService<ChatWindow>();
            win.OnQuit = () =>
            {
                Application.RequestStop();
            };
            top.Add(win);
            top.Add(win.CreateMenuBar());
            Application.Refresh();
        }
    }
}