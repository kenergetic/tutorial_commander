using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Commander
{
    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        // TODO: Move this to the database
        private readonly IDictionary<string, string> users = new Dictionary<string, string>
        {
            { "admin", "pass" },
            { "user", "pass" }
        };
        private readonly string key;


        // Private key to encrypt the Jwt token
        public JwtAuthenticationManager(string key) 
        {
            this.key = key;
        }

        public string Authenticate(string username, string password)
        {
            if (!users.Any(u => u.Key == username && u.Value == password))
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(key);

            // Define the token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, username),
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                // How the token is signed
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(tokenKey), 
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
    }
}