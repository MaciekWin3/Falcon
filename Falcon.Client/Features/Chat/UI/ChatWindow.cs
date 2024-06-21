using Falcon.Client.Features.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Terminal.Gui;

namespace Falcon.Client.Features.Chat.UI
{
    public class ChatWindow : Window
    {
        private static readonly List<string> messages = new();
        private static List<string> users = new();
        private static ListView chatListView;
        private static ListView userList;
        private static TextField chatMessage;
        private readonly SignalRClient signalRClient;
        private readonly ChatService chatService;

        public ChatWindow(SignalRClient signalRClient, ChatService chatService) : base("Falcon")
        {
            this.signalRClient = signalRClient;
            this.chatService = chatService;
            X = 0;
            Y = 1;
            Width = Dim.Fill();
            Height = Dim.Fill();
            Setup("Chat");
            this.signalRClient.OnReceiveMessage += OnReceiveMessageListener;
            this.signalRClient.OnConnect += OnConnectLister;
            this.signalRClient.OnDisconnect += OnDisconnectLister;
            chatListView.SetSource(messages);
        }

        private async void OnConnectLister()
        {
            users = await chatService.GetUsersAsync();
            Application.Refresh();
        }

        private void OnDisconnectLister(string username)
        {
            users.Remove(username);
            Application.Refresh();
        }

        private void OnReceiveMessageListener(string userName, string message)
        {
            ConnectionLister(userName, message);
        }

        private static void ConnectionLister(string userName, string message)
        {
            messages.Add($"{userName}: {message}");
            chatListView.MoveEnd();
            chatListView.GetCurrentHeight(out int h);
            chatListView.ScrollUp(h - 1);
            Application.Refresh();
        }

        public MenuBar CreateMenuBar()
        {
            return new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem("App", new MenuItem []
                {
                    new MenuItem("Quit", "Quit App", () => Application.RequestStop(), null, null, Key.CtrlMask | Key.Q)
                })
            });
        }

        public void ExecuteCommand(string command)
        {
            switch (command)
            {
                case "/clear":
                    messages.Clear();
                    chatMessage.Text = string.Empty;
                    chatListView.MovePageUp();
                    break;
                default:
                    break;
            }
        }

        public void Setup(string text)
        {
            var chatViewFrame = new FrameView(text)
            {
                X = 0,
                Y = 0,
                Width = Dim.Percent(75),
                Height = Dim.Percent(80),
            };

            chatListView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                CanFocus = false,
                AllowsMarking = false,
            };

            chatViewFrame.Add(chatListView);
            Add(chatViewFrame);

            var userListFrame = new FrameView("Online Users")
            {
                X = Pos.Right(chatViewFrame),
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };
            userList = new ListView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                CanFocus = false
            };
            userListFrame.Add(userList);
            Add(userListFrame);

            var chatBar = new FrameView("Message")
            {
                X = 0,
                Y = Pos.Bottom(chatViewFrame),
                Width = chatViewFrame.Width,
                Height = Dim.Fill()
            };

            chatMessage = new TextField(string.Empty)
            {
                X = 0,
                Y = Pos.Center(),
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            // Test
            userList.SetSource(users);

            chatMessage.KeyPress += (a) =>
            {
                if (a.KeyEvent.Key == Key.Enter)
                {
                    string message = chatMessage.Text.ToString();
                    if (!string.IsNullOrEmpty(message) && message[0] == '/')
                    {
                        ExecuteCommand(message);
                    }
                    else
                    {
                        messages.Add($"You: {message ?? ""}");
                        signalRClient.connection.InvokeCoreAsync("SendGroupMessageAsync", args: new[] { message });
                        chatMessage.Text = string.Empty;
                        chatListView.MoveEnd();
                        chatListView.GetCurrentHeight(out int h);
                        chatListView.ScrollUp(h - 1);
                        a.Handled = true;
                    }
                }
            };
            chatBar.Add(chatMessage);
            Add(chatBar);
        }
    }
}