using Terminal.Gui;

namespace Falcon.Client.Windows
{
    public class RoomWindow : Window
    {
        public ListView RoomListView;
        private readonly IList<string> rooms;
        public Action<string> OnChatOpen { get; set; }

        public RoomWindow(IList<string> rooms) : base("Choose Room")
        {
            X = Pos.Center();
            Y = Pos.Center();
            Width = Dim.Percent(100);
            Height = Dim.Percent(100);
            this.rooms = rooms;
            Setup();
        }

        public void Setup()
        {
            RoomListView = new ListView(rooms.ToList())
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = true,
            };

            RoomListView.OpenSelectedItem += OpenChat;
            Add(RoomListView);
        }

        public void OpenChat(EventArgs e)
        {
            string room = RoomListView.SelectedItem.ToString();
            OnChatOpen?.Invoke(room);
        }
    }
}