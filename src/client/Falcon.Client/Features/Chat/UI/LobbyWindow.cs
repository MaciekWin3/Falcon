using Terminal.Gui;

namespace Falcon.Client.Features.Lobby.UI
{
    public class LobbyWindow : Window
    {
        public ListView RoomListView;
        private readonly IList<string> rooms;
        public Action<string> OnChatOpen { get; set; }

        public LobbyWindow()
        {
            Title = "Choose Room";
            X = 0;
            Y = 1;
            Width = Dim.Fill();
            Height = Dim.Fill();
            rooms = new List<string> { "All", "Admins" };
            Setup();
        }

        public void Setup()
        {
            RoomListView = new ListView
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
                AllowsMarking = false,
                CanFocus = true,
                Data = rooms.ToList()
            };

            RoomListView.OpenSelectedItem += OpenChat;

            //Add(RoomListView);
            var button = new Button
            {
                Text = "Open Chat",
                X = Pos.Center(),
                Y = Pos.Center()
            };

            button.Accept += (_, _) => OpenChat(this, null);
            Add(button);
        }

        public MenuBar CreateMenuBar()
        {
            return new MenuBar
            {
                Data = new MenuBarItem[]
                {
                    new MenuBarItem("_App", new MenuItem[]
                    {
                        new MenuItem("_Quit", "", () => Application.RequestStop())
                    })
                }
            };
        }

        public void OpenChat(object sender, ListViewItemEventArgs e)
        {
            string room = RoomListView.SelectedItem.ToString();
            OnChatOpen?.Invoke(room);
        }
    }
}