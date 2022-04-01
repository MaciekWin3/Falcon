using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Falcon.Server.Features.Auth.Models
{
    public class UserToken
    {
        public string Token { get; protected set; }
        public DateTime Expiration { get; protected set; }

        public UserToken(string email, string configurationKey)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, email),
                new Claim(ClaimTypes.Name, email),
                new Claim("value", "falcon"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddMonths(1);

            JwtSecurityToken token = new(
               issuer: null,
               audience: null,
               claims: claims,
               expires: expiration,
               signingCredentials: creds
            );

            Token = new JwtSecurityTokenHandler().WriteToken(token);
            Expiration = expiration;
        }
    }
}