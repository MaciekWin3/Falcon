using Falcon.Server.Features.Auth.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Falcon.Server.Features.Auth.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        public AuthController()
        {
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserToken>> Register(UserDTO userDTO)
        {
            return null;
        }
    }
}