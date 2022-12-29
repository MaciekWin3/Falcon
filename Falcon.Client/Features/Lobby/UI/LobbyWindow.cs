using Terminal.Gui;

namespace Falcon.Client.Features.Lobby.UI
{
    public class LobbyWindow : Window
    {
        public ListView RoomListView;
        private readonly IList<string> rooms;
        public Action<string> OnChatOpen { get; set; }
        public Action OnQuit { get; set; }

        public LobbyWindow(IList<string> rooms) : base("Choose Room")
        {
            X = 0;
            Y = 1;
            Width = Dim.Fill();
            Height = Dim.Fill();
            this.rooms = rooms;
            Setup();
        }

        public void Setup()
        {
            RoomListView = new ListView(rooms.ToList())
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = true,
            };

            RoomListView.OpenSelectedItem += OpenChat;
            Add(RoomListView);
        }

        public MenuBar CreateMenuBar()
        {
            return new MenuBar(new MenuBarItem[]
            {
                new MenuBarItem("App", new MenuItem []
                {
                    new MenuItem("Quit", "Quit App", () => OnQuit?.Invoke(), null, null, Key.Q | Key.CtrlMask)
                })
            });
        }

        public void OpenChat(EventArgs e)
        {
            string room = RoomListView.SelectedItem.ToString();
            OnChatOpen?.Invoke(room);
        }
    }
}