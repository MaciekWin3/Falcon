﻿using Falcon.Client.Features.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Terminal.Gui;

//using Attribute = Terminal.Gui.Attribute;

namespace Falcon.Client.Features.Chat.UI
{
    public class ChatWindow : Window
    {
        private string _username = "User";
        private static readonly List<string> users = new();
        private static readonly List<string> messages = new();
        private static ListView chatListView;
        private static ListView userList;
        private static TextField chatMessage;

        public Action OnQuit { get; set; }

        private readonly SignalRClient signalRClient;

        public ChatWindow(SignalRClient signalRClient) : base("Falcon")
        {
            this.signalRClient = signalRClient;
            X = 0;
            Y = 1;
            Width = Dim.Fill();
            Height = Dim.Fill();
            Setup("Chat");
            this.signalRClient.OnReceiveMessage += OnReceiveMessageListener;
        }

        private void OnReceiveMessageListener(string userName, string message)
        {
            //ThreadStart thread = () => ConnectionLister(userName, message);
            //lock (mutex)
            //{
            //    if (main == null)
            //    {
            //        main = new Thread(thread);
            //    }
            //    main.Start();
            //}
            ConnectionLister(userName, message);
        }

        private static void ConnectionLister(string userName, string message)
        {
            messages.Add($"{userName}: {message}");
            chatListView.SetSource(messages);
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
                    new MenuItem("Quit", "Quit App", () => OnQuit?.Invoke(), null, null, Key.CtrlMask | Key.Q)
                })
            });
        }

        public void ExecuteCommand(string command)
        {
            switch (command)
            {
                case "/clear":
                    messages.Clear();
                    chatListView.SetSource(messages);
                    chatMessage.Text = string.Empty;
                    chatListView.MovePageUp();
                    /*
                    chatListView.MoveEnd();
                    chatListView.GetCurrentHeight(out int h);
                    chatListView.ScrollUp(h - 1);
                    */
                    break;

                default:
                    break;
            }
        }

        private void chatListViewRender(ListViewRowEventArgs obj)
        {
            //obj.RowAttribute = new Attribute(Color.BrightMagenta, Color.Green);
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
                CanFocus = false
            };
            chatListView.RowRender += chatListViewRender;

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
            users.Add(_username);
            users.Add("Konrad");
            userList.SetSource(users);

            KeyDown += (a) =>
            {
                if (a.KeyEvent.ToString().ToLower().Contains("enter"))
                {
                    string message = chatMessage.Text.ToString();
                    if (!string.IsNullOrEmpty(message) && message[0] == '/')
                    {
                        ExecuteCommand(message);
                    }
                    else
                    {
                        messages.Add($"{_username}: {message}");
                        signalRClient.connection.InvokeCoreAsync("SendGroupMessageAsync", args: new[] { message });
                        chatListView.SetSource(messages);
                        chatMessage.Text = string.Empty;
                        chatListView.MoveEnd();
                        chatListView.GetCurrentHeight(out int h);
                        chatListView.ScrollUp(h - 1);
                    }
                }
            };
            chatBar.Add(chatMessage);
            Add(chatBar);
        }
    }
}