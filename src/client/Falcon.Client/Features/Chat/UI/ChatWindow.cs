using Falcon.Client.Features.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using Terminal.Gui;

namespace Falcon.Client.Features.Chat.UI
{
    public sealed class ChatWindow : Window
    {
        const int CHAT_WINDOW_HEIGHT = 3;

        // Components
        private static ListView roomsListView;
        private static ListView chatListView;
        private static ListView userListView;
        private static TextField chatMessagePromptView;

        // Data
        private List<string> messages = [];
        private HashSet<string> users = [];
        private HashSet<string> rooms = ["Admins", "Users"];

        // Services
        private readonly SignalRClient signalRClient;
        private readonly ChatService chatService;

        public ChatWindow(SignalRClient signalRClient, ChatService chatService)
        {
            // Services
            this.signalRClient = signalRClient;
            this.chatService = chatService;

            // Window
            X = 0;
            Y = 1;
            Width = Dim.Fill();
            Height = Dim.Fill();

            Setup();

            // SignalR
            this.signalRClient.OnReceiveMessage += AddMessageToChat;
            this.signalRClient.OnConnected += async () => await HandleConnectionStatusChangeAsync();
            this.signalRClient.OnDisconnected += async () => await HandleConnectionStatusChangeAsync();

            // Initialize data asynchronously
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // TODO: For future implementation
                //var initialMessages = await chatService.GetMessagesAsync();
                var activeRooms = await signalRClient.connection.InvokeAsync<HashSet<string>>("ShowActiveRooms");
                var activeUsers = await signalRClient.connection.InvokeAsync<HashSet<string>>("ShowActiveUsers");

                // Update UI sources
                Application.Invoke(() =>
                {
                    chatListView.SetSource(new ObservableCollection<string>(messages));

                    rooms.Clear();
                    foreach (var room in activeRooms)
                    {
                        rooms.Add(room);
                    }

                    roomsListView.SetSource(new ObservableCollection<string>(rooms));

                    users.Clear();
                    foreach (var user in activeUsers)
                    {
                        users.Add(user);
                    }
                    userListView.SetSource(new ObservableCollection<string>(users));
                });
            }
            catch (Exception ex)
            {
                MessageBox.ErrorQuery(40, 10, "Error", ex.Message, "Ok");
            }
        }

        #region User interface
        public void Setup()
        {
            // Rooms
            var roomsListFrame = new FrameView
            {
                Title = "Rooms",
                X = 0,
                Y = 0,
                Width = Dim.Percent(20),
                Height = Dim.Fill(),
            };

            roomsListView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            roomsListFrame.Add(roomsListView);
            Add(roomsListFrame);

            // Chat
            var chatFrameView = new FrameView
            {
                Title = "Chat",
                X = Pos.Right(roomsListFrame),
                Y = 0,
                Width = Dim.Percent(60),
                Height = Dim.Fill() - CHAT_WINDOW_HEIGHT,
            };

            chatListView = new ListView
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            chatFrameView.Add(chatListView);
            Add(chatFrameView);


            // Users
            var userListFrame = new FrameView
            {
                Title = "Users",
                X = Pos.Right(chatFrameView),
                Y = 0,
                Width = Dim.Percent(20),
                Height = Dim.Fill(),
            };

            userListView = new ListView()
            {
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            userListFrame.Add(userListView);
            Add(userListFrame);

            var chatBar = new FrameView
            {
                Title = "Message",
                X = Pos.Right(roomsListFrame),
                Y = Pos.Bottom(chatFrameView),
                Width = Dim.Percent(60),
                Height = CHAT_WINDOW_HEIGHT
            };

            chatMessagePromptView = new TextField
            {
                X = 0,
                Y = Pos.Center(),
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            // Test
            userListView.SetSource(new ObservableCollection<string>(users));

            chatMessagePromptView.KeyDown += (_, a) =>
            {
                if (a.KeyCode == Key.Enter)
                {
                    string message = chatMessagePromptView.Text.ToString();
                    if (!string.IsNullOrEmpty(message) && message[0] == '/')
                    {
                        ExecuteCommand(message);
                        chatMessagePromptView.Text = string.Empty;
                        a.Handled = true;
                    }
                    else
                    {
                        AddMessageToChat("You", message);
                        signalRClient.connection.InvokeCoreAsync("SendGroupMessageAsync", args: [message]);
                        chatMessagePromptView.Text = string.Empty;
                        a.Handled = true;
                    }
                }
            };

            chatBar.Add(chatMessagePromptView);
            Add(chatBar);
            //Add(CreateMenuBar());
        }

        public MenuBar CreateMenuBar()
        {
            // TODO: Add menu bar
            return new MenuBar
            {
                Title = "TestMenuBar",
                Menus =
                [
                    new MenuBarItem("_App", new MenuItem[]
                    {
                        new MenuItem("_Quit", "", () => Application.RequestStop(), null, null)
                    })
                ]
            };
        }

        #endregion

        private async Task HandleConnectionStatusChangeAsync()
        {
            try
            {
                var activeUsers = await FetchActiveUsersAsync();
                userListView.SetSource(new ObservableCollection<string>(activeUsers));
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error during connection status change: {ex.Message}");
            }
        }

        private async Task<HashSet<string>> FetchActiveUsersAsync()
        {
            try
            {
                return await signalRClient.connection.InvokeAsync<HashSet<string>>("ShowActiveUsers");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching active users: {ex.Message}");
                return new HashSet<string>();
            }
        }

        public void ExecuteCommand(string command)
        {
            switch (command)
            {
                case "/clear":
                    messages.Clear();
                    chatListView.MovePageUp();
                    break;
                default:
                    break;
            }
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