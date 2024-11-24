using Falcon.Client.Features.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
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

        public ChatWindow(SignalRClient signalRClient, ChatService chatService)
        {
            this.signalRClient = signalRClient;
            this.chatService = chatService;
            Title = "Falon";
            X = 0;
            Y = 1;
            Width = Dim.Fill();
            Height = Dim.Fill();
            Setup("Chat");
            this.signalRClient.OnReceiveMessage += AddMessageToChat;
            this.signalRClient.OnConnect += OnConnectLister;
            this.signalRClient.OnDisconnect += OnDisconnectLister;
            chatListView.SetSource(new ObservableCollection<string>(messages));
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

        public MenuBar CreateMenuBar()
        {
            return new MenuBar
            {
                Data = new MenuBarItem[]
                {
                    new MenuBarItem("_App", new MenuItem[]
                    {
                        new MenuItem("_Quit", "", () => Application.RequestStop(), null, null)
                    })
                }
            };
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
            var chatViewFrame = new FrameView
            {
                Title = text,
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

            var userListFrame = new FrameView
            {
                Text = "Online Users",
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

            var chatBar = new FrameView
            {
                Title = "Message",
                X = 0,
                Y = Pos.Bottom(chatViewFrame),
                Width = chatViewFrame.Width,
                Height = Dim.Fill()
            };

            chatMessage = new TextField
            {
                X = 0,
                Y = Pos.Center(),
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            // Test
            userList.SetSource(new ObservableCollection<string>(users));

            chatMessage.KeyDown += (_, a) =>
            {
                if (a.KeyCode == Key.Enter)
                {
                    string message = chatMessage.Text.ToString();
                    if (!string.IsNullOrEmpty(message) && message[0] == '/')
                    {
                        ExecuteCommand(message);
                    }
                    else
                    {
                        AddMessageToChat("You", message);
                        signalRClient.connection.InvokeCoreAsync("SendGroupMessageAsync", args: new[] { message });
                        chatMessage.Text = string.Empty;
                        a.Handled = true;
                    }
                }
            };
            chatBar.Add(chatMessage);
            Add(chatBar);
        }

        private void AddMessageToChat(string user, string message)
        {
            messages.Add($"{user}: {message ?? ""}");
            chatListView.SetSource(new ObservableCollection<string>(messages));
            chatListView.MoveEnd();
            Application.Refresh();
        }
    }
}