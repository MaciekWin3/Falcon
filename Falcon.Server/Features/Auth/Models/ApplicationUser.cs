using Falcon.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Falcon.Server.Features.Auth.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Message> Messages { get; set; }
    }
}