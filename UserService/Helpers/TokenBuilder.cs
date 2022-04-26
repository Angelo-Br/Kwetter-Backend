using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserService.Helpers
{
    public class TokenBuilder
    {
        public string BuildToken(int userId)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("KwetterSecretJWTTokenForAuthentication"));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            };
            var jwt = new JwtSecurityToken(claims: claims, signingCredentials: signingCredentials, expires: DateTime.Now.AddMinutes(5), issuer: "http://localhost:5001", audience: "http://localhost:5001");
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }
    }
}
