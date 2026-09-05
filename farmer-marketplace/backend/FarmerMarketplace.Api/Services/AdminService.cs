// backend/FarmerMarketplace.Api/Services/AdminService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class AdminService:IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserResponseDto>> GetUsersAsync(Guid requestingUserId, string? role)
        {
            IQueryable<User> query = _context.Users.AsNoTracking();

            if (role == nameof(UserRole.FpoAdmin))
            {
                // FpoAdmin only sees farmers linked under their own FPO, not the whole platform
                query = query.Where(u => u.FpoId == requestingUserId || u.Id == requestingUserId);
            }
            // PlatformAdmin (or any other allowed role) sees everyone — no filter applied

            var users = await query
                .OrderBy(u => u.Name)
                .ToListAsync();

            return users.Select(MapToResponseDto).ToList();
        }

        public async Task<AdminSummaryDto> GetSummaryAsync(Guid requestingUserId, string? role)
        {
            IQueryable<User> query = _context.Users.AsNoTracking();

            if (role == nameof(UserRole.FpoAdmin))
            {
                query = query.Where(u => u.FpoId == requestingUserId);
            }

            var totalFarmers = await query.CountAsync(u => u.Role == UserRole.Farmer);
            var totalBuyers = await query.CountAsync(u => u.Role == UserRole.Buyer);
            var totalFpoAdmins = role == nameof(UserRole.FpoAdmin)
                ? 0 // an FpoAdmin scoped to their own farmers has no other FpoAdmins to count
                : await _context.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.FpoAdmin);

            return new AdminSummaryDto
            {
                TotalFarmers = totalFarmers,
                TotalBuyers = totalBuyers,
                TotalFpoAdmins = totalFpoAdmins,
                TotalProducts = 0,   // TODO: populate once ProductService exists
                TotalOrders = 0,     // TODO: populate once OrderService exists
                PendingOrders = 0    // TODO: populate once OrderService exists
            };
        }

        private static UserResponseDto MapToResponseDto(User user)
        {
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