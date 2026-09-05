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

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDto> UpdateProfileAsync(Guid id, Guid requestingUserId, ProfileSetupDto dto)
        {
            if (id != requestingUserId)
                throw new UnauthorizedAccessException("You can only update your own profile.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            user.Village = dto.Village;
            user.District = dto.District;
            user.State = dto.State;
            user.Pincode = dto.Pincode;
            user.Latitude = dto.Latitude;
            user.Longitude = dto.Longitude;
            user.Region = dto.Region;
            user.PrimaryCrops = dto.PrimaryCrops;
            user.BankAccountNumber = dto.BankAccountNumber;
            user.BankIfsc = dto.BankIfsc;
            user.AccountHolderName = dto.AccountHolderName;
            user.UpiId = dto.UpiId;
            user.BusinessName = dto.BusinessName;
            user.GstNumber = dto.GstNumber;
            user.DeliveryAddress = dto.DeliveryAddress;
            user.IsProfileComplete = true;
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
                FpoId = user.FpoId
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
                     Village = user.Village,
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
        FpoId = user.FpoId
    };
}
    }
}