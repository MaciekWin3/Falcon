using Falcon.Server.Features.Auth.Models;
using Falcon.Server.Features.Messages.Models;

namespace Falcon.Server.Features.Rooms.Models
{
    public class Room
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastMessageSent { get; set; }
        public virtual ApplicationUser Owner { get; set; }
        public virtual List<Message> Messages { get; set; }
    }
}