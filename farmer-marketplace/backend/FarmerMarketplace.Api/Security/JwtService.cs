// backend/FarmerMarketplace.Api/Security/JwtService.cs

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FarmerMarketplace.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace FarmerMarketplace.Api.Security
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public (string Token, string Jti, DateTime Expires) GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jti = Guid.NewGuid().ToString();
            var expires = DateTime.UtcNow.AddDays(7);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), jti, expires);
        }
    }
}