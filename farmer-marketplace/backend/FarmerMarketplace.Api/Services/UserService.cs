// backend/FarmerMarketplace.Api/Services/UserService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserService(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<UserResponseDto> GetProfileAsync(Guid id, Guid requestingUserId)
        {
            if (id != requestingUserId)
                throw new UnauthorizedAccessException("You can only view your own profile.");

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return new UserResponseDto
            {
                Id = user.Id, Name = user.Name, Email = user.Email, Phone = user.Phone,
                Role = user.Role, Location = user.Location, PreferredLanguage = user.PreferredLanguage,
                FpoId = user.FpoId, IsProfileComplete = user.IsProfileComplete, CreatedAt = user.CreatedAt,
                Address = user.Address, District = user.District, State = user.State, Pincode = user.Pincode,
                PrimaryCrops = user.PrimaryCrops, BankAccountNumber = user.BankAccountNumber,
                BankIfsc = user.BankIfsc, AccountHolderName = user.AccountHolderName, UpiId = user.UpiId,
                BusinessName = user.BusinessName, GstNumber = user.GstNumber, DeliveryAddress = user.DeliveryAddress,
                Latitude = user.Latitude, Longitude = user.Longitude, Region = user.Region
            };
        }

        public async Task<UserResponseDto> UpdateProfileAsync(Guid id, Guid requestingUserId, ProfileSetupDto dto)
        {
            if (id != requestingUserId)
                throw new UnauthorizedAccessException("You can only update your own profile.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.Address = dto.Address;
            user.District = dto.District;
            user.State = dto.State;
            user.Pincode = dto.Pincode;
            user.Latitude = dto.Latitude;
            user.Longitude = dto.Longitude;
            user.Region = dto.Region;
            user.PreferredLanguage = string.IsNullOrWhiteSpace(dto.PreferredLanguage) ? user.PreferredLanguage : dto.PreferredLanguage;
            user.PrimaryCrops = dto.PrimaryCrops;
            user.BankAccountNumber = dto.BankAccountNumber;
            user.BankIfsc = dto.BankIfsc;
            user.AccountHolderName = dto.AccountHolderName;
            user.UpiId = dto.UpiId;
            user.BusinessName = dto.BusinessName;
            user.GstNumber = dto.GstNumber;
            user.DeliveryAddress = dto.DeliveryAddress;
            user.IsProfileComplete = true;

            if (user.Latitude == null || user.Longitude == null)
            {
                var addressParts = new[] { user.Address, user.District, user.State, user.Pincode };
                var fullAddress = string.Join(", ", addressParts.Where(s => !string.IsNullOrWhiteSpace(s)));
                
                if (string.IsNullOrWhiteSpace(fullAddress) && !string.IsNullOrWhiteSpace(user.DeliveryAddress))
                    fullAddress = user.DeliveryAddress; // Fallback for buyers

                var coords = await GeocodeAddressAsync(fullAddress);
                if (coords.HasValue)
                {
                    user.Latitude = coords.Value.Lat;
                    user.Longitude = coords.Value.Lng;
                }
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Phone = user.Phone,
                Location = user.Location,
                PreferredLanguage = user.PreferredLanguage,
                FpoId = user.FpoId,
                IsProfileComplete = user.IsProfileComplete,
                CreatedAt = user.CreatedAt,
                Address = user.Address,
                District = user.District,
                State = user.State,
                Pincode = user.Pincode,
                PrimaryCrops = user.PrimaryCrops,
                BankAccountNumber = user.BankAccountNumber,
                BankIfsc = user.BankIfsc,
                AccountHolderName = user.AccountHolderName,
                UpiId = user.UpiId,
                BusinessName = user.BusinessName,
                GstNumber = user.GstNumber,
                DeliveryAddress = user.DeliveryAddress,
                Latitude = user.Latitude,
                Longitude = user.Longitude,
                Region = user.Region
            };
        }


                // backend/FarmerMarketplace.Api/Services/UserService.cs (add these two methods)

                public async Task<PublicProfileDto> GetPublicProfileAsync(Guid id)
            {   
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                throw new KeyNotFoundException("User not found.");

                return new PublicProfileDto
                {
                     Id = user.Id,
                     Name = user.Name,
                     Role = user.Role,
                     Address = user.Address,
                     District = user.District,
                     State = user.State,
                     Phone = user.Phone,
                     PreferredLanguage = user.PreferredLanguage,
        PrimaryCrops = user.PrimaryCrops
    };
}

public async Task<UserResponseDto> UpdateBasicProfileAsync(Guid id, Guid requestingUserId, UpdateBasicProfileDto dto)
{
    if (id != requestingUserId)
        throw new UnauthorizedAccessException("You can only update your own profile.");

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    if (user == null)
        throw new KeyNotFoundException("User not found.");

    if (!string.IsNullOrWhiteSpace(dto.Name))
        user.Name = dto.Name;

    if (!string.IsNullOrWhiteSpace(dto.Phone))
        user.Phone = dto.Phone;

    if (!string.IsNullOrWhiteSpace(dto.PreferredLanguage))
        user.PreferredLanguage = dto.PreferredLanguage;

    user.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return new UserResponseDto
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        Phone = user.Phone,
        Location = user.Location,
        PreferredLanguage = user.PreferredLanguage,
        FpoId = user.FpoId,
        IsProfileComplete = user.IsProfileComplete,
        CreatedAt = user.CreatedAt,
        Address = user.Address,
        District = user.District,
        State = user.State,
        Pincode = user.Pincode,
        PrimaryCrops = user.PrimaryCrops,
        BankAccountNumber = user.BankAccountNumber,
        BankIfsc = user.BankIfsc,
        AccountHolderName = user.AccountHolderName,
        UpiId = user.UpiId,
        BusinessName = user.BusinessName,
        GstNumber = user.GstNumber,
        DeliveryAddress = user.DeliveryAddress,
        Latitude = user.Latitude,
        Longitude = user.Longitude,
        Region = user.Region
    };
}

        private async Task<(double Lat, double Lng)?> GeocodeAddressAsync(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FasalConnect/1.0 profile-setup");

            try
            {
                var url = $"https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&countrycodes=in&q={Uri.EscapeDataString(address)}";
                using var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;
                using var document = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                var result = document.RootElement.EnumerateArray().FirstOrDefault();
                if (result.ValueKind == System.Text.Json.JsonValueKind.Undefined) return null;
                var lat = result.TryGetProperty("lat", out var latProperty) ? latProperty.GetString() : null;
                var lng = result.TryGetProperty("lon", out var lngProperty) ? lngProperty.GetString() : null;
                if (double.TryParse(lat, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var latitude)
                    && double.TryParse(lng, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var longitude))
                    return (latitude, longitude);
            }
            catch
            {
                // ignore
            }
            return null;
        }
    }
}