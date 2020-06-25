using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Commander.Auth
{
    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        // TODO: Move this to a database
        private readonly List<User> users = new List<User> {
            new User { UserName = "admin", Password = "pass", Role = "Administrator" },
            new User { UserName = "user", Password = "pass", Role = "User" }
        };

        private readonly string key;


        // Private key to encrypt the Jwt token
        public JwtAuthenticationManager(string key) 
        {
            this.key = key;
        }

        public string Authenticate(string username, string password)
        {
            if (!users.Any(u => u.UserName == username && u.Password == password))
            {
                return null;
            }
            var user = users.First(u => u.UserName == username && u.Password == password);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.ASCII.GetBytes(key);

            // Define the token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role)
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