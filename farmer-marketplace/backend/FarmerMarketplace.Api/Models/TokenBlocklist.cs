// backend/FarmerMarketplace.Api/Models/TokenBlocklist.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.Models
{
    // Supports POST /auth/logout — stores invalidated JWT ids (jti) until they'd
    // have naturally expired anyway, so the blocklist doesn't grow forever.
    public class TokenBlocklist
    {
        [Key]
        public string Jti { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}