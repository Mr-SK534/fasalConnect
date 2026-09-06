// backend/FarmerMarketplace.Api/Services/AdminService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class AdminService : IAdminService
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


        // backend/FarmerMarketplace.Api/Services/AdminService.cs — updated GetSummaryAsync

        public async Task<AdminSummaryDto> GetSummaryAsync(Guid requestingUserId, string? role)
        {
            IQueryable<User> userQuery = _context.Users.AsNoTracking();
            IQueryable<Product> productQuery = _context.Products.AsNoTracking();
            IQueryable<Order> orderQuery = _context.Orders.AsNoTracking();

            if (role == nameof(UserRole.FpoAdmin))
            {
                // Scope everything to farmers linked under this FPO
                userQuery = userQuery.Where(u => u.FpoId == requestingUserId);

                var linkedFarmerIds = await _context.Users
                    .Where(u => u.FpoId == requestingUserId && u.Role == UserRole.Farmer)
                    .Select(u => u.Id)
                    .ToListAsync();

                productQuery = productQuery.Where(p => linkedFarmerIds.Contains(p.FarmerId));

                orderQuery = orderQuery.Where(o =>
                    _context.OrderItems.Any(i => i.OrderId == o.Id && linkedFarmerIds.Contains(i.FarmerId)));
            }

            var totalFarmers = await userQuery.CountAsync(u => u.Role == UserRole.Farmer);
            var totalBuyers = await userQuery.CountAsync(u => u.Role == UserRole.Buyer);
            var totalFpoAdmins = role == nameof(UserRole.FpoAdmin)
                ? 0
                : await _context.Users.AsNoTracking().CountAsync(u => u.Role == UserRole.FpoAdmin);

            var totalProducts = await productQuery.CountAsync(p => p.IsActive);
            var totalOrders = await orderQuery.CountAsync();
            var pendingOrders = await orderQuery.CountAsync(o => o.Status == OrderStatus.Pending);

            return new AdminSummaryDto
            {
                TotalFarmers = totalFarmers,
                TotalBuyers = totalBuyers,
                TotalFpoAdmins = totalFpoAdmins,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders
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