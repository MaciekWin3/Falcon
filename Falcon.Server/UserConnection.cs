namespace Falcon.Server
{
    public class UserConnection
    {
        public string Username { get; set; }
        public string Room { get; set; }

        public UserConnection(string user, string room)
        {
            Username = user;
            Room = room;
        }
    }
}