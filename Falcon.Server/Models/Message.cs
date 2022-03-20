using Falcon.Server.Features.Auth.Models;

namespace Falcon.Server.Models
{
    public class Message
    {
        public long Id { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string Content { get; set; }
        public string RoomId { get; set; }
        public DateTime Date { get; set; }
    }
}