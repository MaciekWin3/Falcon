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
    internal class TerminalOrchestrator
    {
        private Func<Task> running;
        private readonly ChatService chatService;
        private readonly AuthService authService;
        private readonly SignalRClient signalRClient;
        private readonly IServiceProvider serviceProvider;
        private static Dictionary<string, string> parameters { get; set; }

        public TerminalOrchestrator(ChatService chatService, AuthService authService,
            SignalRClient signalRClient, IServiceProvider serviceProvider)
        {
            running = ShowLoginWindow;
            parameters = new Dictionary<string, string>();
            this.chatService = chatService;
            this.authService = authService;
            this.signalRClient = signalRClient;
            this.serviceProvider = serviceProvider;
        }

        public async Task Run()
        {
            Application.Init();
            Colors.Base.Normal = Application.Driver.MakeAttribute(Color.BrightGreen, Color.Black);
            Console.OutputEncoding = System.Text.Encoding.Default;
            while (running != null)
            {
                await running.Invoke();
            }
            Application.Shutdown();
        }

        private Task ShowLoginWindow()
        {
            var top = Application.Top;
            var win = new LoginWindow
            {
                OnAuthorize = async (authData) =>
                {
                    return await authService.Login(authData.name, authData.password);
                },

                OnLogin = async (token) =>
                {
                    // Todo: delete
                    // await chatService.RunAsync(token);
                    await signalRClient.StartConnectionAsync(token);
                    Application.MainLoop.Invoke(() =>
                    {
                        running = ShowRoomWindow;
                        Application.RequestStop();
                    });
                },

                OnExit = () =>
                {
                    running = null;
                    //top.Running = false;
                    Application.RequestStop();
                },

                OnQuit = () =>
                {
                    running = null;
                    Application.RequestStop();
                },
            };

            top.Add(win);
            top.Add(win.CreateMenuBar());
            Application.Run();
            return Task.CompletedTask;
        }

        private Task ShowChatWindow()
        {
            var top = Application.Top;
            var win = serviceProvider.GetService<ChatWindow>();
            win.OnQuit = () =>
            {
                running = null;
                //top.Running = false;
                Application.RequestStop();
            };
            top.Add(win);
            top.Add(win.CreateMenuBar());
            Application.Run();
            return Task.CompletedTask;
        }

        private Task ShowRoomWindow()
        {
            // Sprawdzić synchroniczną metode
            var top = Application.Top;
            Application.MainLoop.Invoke(async () =>
            {
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
                        await signalRClient.connection.InvokeCoreAsync("JoinRoom", args: new[] { room });
                        Application.MainLoop.Invoke(() =>
                        {
                            running = ShowChatWindow;
                            Application.RequestStop();
                        });
                    },

                    OnQuit = () =>
                    {
                        running = null;
                        Application.RequestStop();
                    }
                };
                top.Add(win);
                top.Add(win.CreateMenuBar());
            });
            Application.Run();
            return Task.CompletedTask;
        }
    }
}