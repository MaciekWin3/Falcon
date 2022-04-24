using Falcon.Server.Features.Auth.Models;
using Falcon.Server.Features.Rooms.Models;

namespace Falcon.Server.Features.Messages.Models
{
    public class Message
    {
        public long Id { get; set; }
        public virtual ApplicationUser User { get; set; }
        public string Content { get; set; }
        public virtual Room Room { get; set; }
    }
}