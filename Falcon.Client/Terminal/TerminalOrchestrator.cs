using Falcon.Client.Services;
using Falcon.Client.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace Falcon.Client.Terminal
{
    internal class TerminalOrchestrator : ITerminalOrchestrator
    {
        private Func<Task> running;
        private readonly ChatService2 chatService;
        private readonly AuthService2 authService;
        private readonly SignalRClient signalRClient;
        private readonly IServiceProvider serviceProvider;
        private static Dictionary<string, string> parameters { get; set; }

        public TerminalOrchestrator(ChatService2 chatService, AuthService2 authService,
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
            //Colors.Base.Normal = Application.Driver.MakeAttribute(Color.BrightBlue, Color.Black);
            Console.OutputEncoding = System.Text.Encoding.Default;
            while (running != null)
            {
                await running.Invoke();
            }
            Application.Shutdown();
        }

        private async Task ShowLoginWindow()
        {
            var top = Application.Top;
            // var win = authService.CreateLoginWindow();
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
                    parameters.Add("token", token);
                    Application.MainLoop.Invoke(() =>
                    {
                        running = ShowRoomWindow;
                        Application.RequestStop();
                    });
                },

                OnExit = () =>
                {
                    running = null;
                    top.Running = false;
                },
            };

            top.Add(win);
            Application.Run();
        }

        private async Task ShowChatWindow()
        {
            var top = Application.Top;
            var win = serviceProvider.GetService<ChatWindow>();
            win.OnQuit = () =>
            {
                running = null;
                Application.RequestStop();
            };
            top.Add(win);
            Application.Run();
            // Handle error
        }

        private async Task ShowRoomWindow()
        {
            // Sprawdzić synchroniczną metode
            var top = Application.Top;
            Application.MainLoop.Invoke(async () =>
            {
                IList<string> rooms = new List<string>();
                rooms = await chatService.GetListOfRoomAsync();
                var win = new RoomWindow(rooms)
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
                    }
                };
                top.Add(win);
            });
            Application.Run();
        }
    }
}