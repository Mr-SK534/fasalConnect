// backend/FarmerMarketplace.Api/DTOs/LoginDto.cs

using System.ComponentModel.DataAnnotations;

namespace FarmerMarketplace.Api.DTOs
{
    public class LoginDto
    {
        // v2: accepts either email or phone
        [Required]
        public string EmailOrPhone { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}