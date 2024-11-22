using Falcon.Server.Features.Messages.Models;
using Falcon.Server.Features.Rooms.Models;
using Microsoft.AspNetCore.Identity;

namespace Falcon.Server.Features.Auth.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual IList<Message> Messages { get; set; }
        public virtual IList<Room> OwnedRooms { get; set; }
    }
}