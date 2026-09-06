// backend/FarmerMarketplace.Api/Services/FpoService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using FarmerMarketplace.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class FpoService: IFpoService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;

        public FpoService(AppDbContext context, PasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<UserResponseDto>> GetLinkedFarmersAsync(Guid fpoId, Guid requestingUserId, string? role)
        {
            await EnsureFpoAccess(fpoId, requestingUserId, role);

            var farmers = await _context.Users
                .AsNoTracking()
                .Where(u => u.FpoId == fpoId && u.Role == UserRole.Farmer)
                .OrderBy(u => u.Name)
                .ToListAsync();

            return farmers.Select(MapToResponseDto).ToList();
        }

        public async Task<UserResponseDto> LinkFarmerAsync(Guid fpoId, Guid requestingUserId, LinkFarmerDto dto)
        {
            await EnsureFpoAccess(fpoId, requestingUserId, role: nameof(UserRole.FpoAdmin));

            if (!dto.FarmerId.HasValue && string.IsNullOrWhiteSpace(dto.FarmerEmail))
                throw new ArgumentException("Provide either farmerId or farmerEmail.");

            var farmer = dto.FarmerId.HasValue
                ? await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.FarmerId.Value)
                : await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email != null && u.Email.ToLower() == dto.FarmerEmail!.ToLower());

            if (farmer == null)
                throw new KeyNotFoundException("Farmer account not found.");

            if (farmer.Role != UserRole.Farmer)
                throw new InvalidOperationException("Only Farmer accounts can be linked to an FPO.");

            if (farmer.FpoId.HasValue && farmer.FpoId != fpoId)
                throw new InvalidOperationException("This farmer is already linked to a different FPO.");

            farmer.FpoId = fpoId;
            farmer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponseDto(farmer);
        }

        public async Task<UserResponseDto> CreateFarmerAsync(Guid fpoId, Guid requestingUserId, CreateFpoFarmerDto dto)
        {
            await EnsureFpoAccess(fpoId, requestingUserId, nameof(UserRole.FpoAdmin));

            var phone = dto.Phone.Trim();
            var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Phone == phone))
                throw new InvalidOperationException("Phone is already registered.");

            if (email != null && await _context.Users.AnyAsync(u => u.Email != null && u.Email.ToLower() == email))
                throw new InvalidOperationException("Email is already registered.");

            var farmer = new User
            {
                Name = dto.Name.Trim(),
                Phone = phone,
                Email = email,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = UserRole.Farmer,
                FpoId = fpoId,
                Village = dto.Village,
                District = dto.District,
                State = dto.State,
                IsProfileComplete = false,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(farmer);
            await _context.SaveChangesAsync();
            return MapToResponseDto(farmer);
        }

        public async Task<FpoEarningsDto> GetEarningsAsync(Guid fpoId, Guid requestingUserId, string? role)
        {
            await EnsureFpoAccess(fpoId, requestingUserId, role);

            var paidItems = await _context.OrderItems
                .AsNoTracking()
                .Include(item => item.Farmer)
                .Where(item => item.Farmer != null && item.Farmer.FpoId == fpoId
                    && _context.Payments.Any(payment => payment.OrderId == item.OrderId && payment.Status == PaymentStatus.Paid))
                .ToListAsync();

            return new FpoEarningsDto
            {
                TotalEarnings = paidItems.Sum(item => item.SubTotal),
                PaidOrders = paidItems.Select(item => item.OrderId).Distinct().Count(),
                LinkedFarmers = await _context.Users.CountAsync(user => user.FpoId == fpoId && user.Role == UserRole.Farmer)
            };
        }

        public async Task UnlinkFarmerAsync(Guid fpoId, Guid farmerId, Guid requestingUserId)
        {
            await EnsureFpoAccess(fpoId, requestingUserId, role: nameof(UserRole.FpoAdmin));

            var farmer = await _context.Users.FirstOrDefaultAsync(u => u.Id == farmerId);

            if (farmer == null)
                throw new KeyNotFoundException("Farmer account not found.");

            if (farmer.FpoId != fpoId)
                throw new InvalidOperationException("This farmer is not linked to this FPO.");

            farmer.FpoId = null;
            farmer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // PlatformAdmin can view any FPO's farmers; FpoAdmin only their own
        private async Task EnsureFpoAccess(Guid fpoId, Guid requestingUserId, string? role)
        {
            var fpo = await _context.Users.FirstOrDefaultAsync(u => u.Id == fpoId && u.Role == UserRole.FpoAdmin);

            if (fpo == null)
                throw new KeyNotFoundException("FPO not found.");

            var isPlatformAdmin = role == nameof(UserRole.PlatformAdmin);
            var isThisFpoAdmin = role == nameof(UserRole.FpoAdmin) && requestingUserId == fpoId;

            if (!isPlatformAdmin && !isThisFpoAdmin)
                throw new UnauthorizedAccessException("You do not have access to this FPO's farmers.");
        }

        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Location = user.Location,
                PreferredLanguage = user.PreferredLanguage,
                FpoId = user.FpoId,
                IsProfileComplete = user.IsProfileComplete,
                CreatedAt = user.CreatedAt
                ,Village = user.Village
                ,District = user.District
                ,State = user.State
                ,PrimaryCrops = user.PrimaryCrops
                ,BankAccountNumber = user.BankAccountNumber
                ,BankIfsc = user.BankIfsc
                ,AccountHolderName = user.AccountHolderName
                ,UpiId = user.UpiId
                ,BusinessName = user.BusinessName
                ,GstNumber = user.GstNumber
                ,DeliveryAddress = user.DeliveryAddress
            };
        }
    }
}