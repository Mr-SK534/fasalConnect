using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;
using FarmerMarketplace.Api.Security;

namespace FarmerMarketplace.Api.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminService(AppDbContext context, PasswordHasher passwordHasher, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<AdminUserListResponseDto> GetUsersAsync(Guid requestingUserId, string? role, string? userRole, string? search, int page, int pageSize)
        {
            EnsurePlatformAdmin(role);
            page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _context.Users.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(userRole) && Enum.TryParse<UserRole>(userRole, true, out var parsedRole)) query = query.Where(user => user.Role == parsedRole);
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(user => user.Name.ToLower().Contains(search.ToLower()) || (user.Email != null && user.Email.ToLower().Contains(search.ToLower())) || (user.Phone != null && user.Phone.Contains(search)));
            var total = await query.CountAsync();
            var users = await query.OrderBy(user => user.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new AdminUserListResponseDto { Items = users.Select(MapUser).ToList(), Page = page, PageSize = pageSize, TotalCount = total, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
        }

        public async Task<UserResponseDto> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id) ?? throw new KeyNotFoundException("User not found.");
            return MapUser(user);
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateAdminUserDto dto)
        {
            var phone = dto.Phone.Trim();
            var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(user => user.Phone == phone))
                throw new InvalidOperationException("An account with this phone number already exists.");
            if (email != null && await _context.Users.AnyAsync(user => user.Email != null && user.Email.ToLower() == email))
                throw new InvalidOperationException("An account with this email already exists.");

            var profileComplete = !string.IsNullOrWhiteSpace(dto.Address)
                || !string.IsNullOrWhiteSpace(dto.District)
                || !string.IsNullOrWhiteSpace(dto.State)
                || !string.IsNullOrWhiteSpace(dto.BusinessName)
                || !string.IsNullOrWhiteSpace(dto.DeliveryAddress)
                || !string.IsNullOrWhiteSpace(dto.PrimaryCrops);

            var user = new User
            {
                Name = dto.Name.Trim(), Phone = phone, Email = email,
                PasswordHash = _passwordHasher.HashPassword(dto.Password), Role = dto.Role,
                PreferredLanguage = string.IsNullOrWhiteSpace(dto.PreferredLanguage) ? "en" : dto.PreferredLanguage,
                Latitude = dto.Latitude, Longitude = dto.Longitude, Region = dto.Region,
                Address = dto.Address, District = dto.District, State = dto.State, Pincode = dto.Pincode,
                PrimaryCrops = dto.PrimaryCrops, BankAccountNumber = dto.BankAccountNumber,
                BankIfsc = dto.BankIfsc, AccountHolderName = dto.AccountHolderName, UpiId = dto.UpiId,
                BusinessName = dto.BusinessName, GstNumber = dto.GstNumber, DeliveryAddress = dto.DeliveryAddress,
                IsProfileComplete = profileComplete, UpdatedAt = DateTime.UtcNow
            };

            if (user.Latitude == null || user.Longitude == null)
            {
                var addressParts = new[] { user.Address, user.District, user.State, user.Pincode };
                var fullAddress = string.Join(", ", addressParts.Where(s => !string.IsNullOrWhiteSpace(s)));
                
                if (string.IsNullOrWhiteSpace(fullAddress) && !string.IsNullOrWhiteSpace(user.DeliveryAddress))
                    fullAddress = user.DeliveryAddress;

                var coords = await GeocodeAddressAsync(fullAddress);
                if (coords.HasValue)
                {
                    user.Latitude = coords.Value.Lat;
                    user.Longitude = coords.Value.Lng;
                }
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return MapUser(user);
        }

        public async Task<UserResponseDto> SuspendUserAsync(Guid id, SuspendUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == id) ?? throw new KeyNotFoundException("User not found.");
            user.Suspended = dto.Suspended; user.SuspensionReason = dto.Suspended ? dto.Reason : null; user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapUser(user);
        }

        public async Task<AdminOrderListResponseDto> GetOrdersAsync(string? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize)
        {
            page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
            var query = _context.Orders.AsNoTracking().Include(order => order.Buyer).Include(order => order.Items).ThenInclude(item => item.Product).Include(order => order.Items).ThenInclude(item => item.Farmer).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus)) query = query.Where(order => order.Status == parsedStatus);
            if (dateFrom.HasValue) query = query.Where(order => order.CreatedAt >= dateFrom.Value);
            if (dateTo.HasValue) query = query.Where(order => order.CreatedAt < dateTo.Value.Date.AddDays(1));
            var total = await query.CountAsync();
            var orders = await query.OrderByDescending(order => order.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new AdminOrderListResponseDto { Items = orders.Select(MapOrder).ToList(), Page = page, PageSize = pageSize, TotalCount = total, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
        }

        public async Task<OrderResponseDto> OverrideOrderStatusAsync(Guid id, OverrideOrderStatusDto dto)
        {
            var order = await _context.Orders.Include(item => item.Buyer).Include(item => item.Items).ThenInclude(item => item.Product).Include(item => item.Items).ThenInclude(item => item.Farmer).FirstOrDefaultAsync(item => item.Id == id) ?? throw new KeyNotFoundException("Order not found.");
            order.Status = dto.Status; order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapOrder(order);
        }

        public async Task<AdminSummaryDto> GetSummaryAsync(Guid requestingUserId, string? role)
        {
            EnsurePlatformAdmin(role);
            return new AdminSummaryDto { TotalFarmers = await _context.Users.CountAsync(user => user.Role == UserRole.Farmer), TotalBuyers = await _context.Users.CountAsync(user => user.Role == UserRole.Buyer), TotalFpoAdmins = await _context.Users.CountAsync(user => user.Role == UserRole.FpoAdmin), TotalProducts = await _context.Products.CountAsync(product => product.IsActive), TotalOrders = await _context.Orders.CountAsync(), PendingOrders = await _context.Orders.CountAsync(order => order.Status == OrderStatus.Pending) };
        }

        private static void EnsurePlatformAdmin(string? role)
        {
            if (role != nameof(UserRole.PlatformAdmin)) throw new UnauthorizedAccessException("PlatformAdmin access required.");
        }

        private static UserResponseDto MapUser(User user) => new()
        {
            Id = user.Id, Name = user.Name, Email = user.Email, Phone = user.Phone, Role = user.Role, Location = user.Location, PreferredLanguage = user.PreferredLanguage, FpoId = user.FpoId, IsProfileComplete = user.IsProfileComplete, CreatedAt = user.CreatedAt, Suspended = user.Suspended, SuspensionReason = user.SuspensionReason, Address = user.Address, District = user.District, State = user.State, Pincode = user.Pincode, Region = user.Region, Latitude = user.Latitude, Longitude = user.Longitude, PrimaryCrops = user.PrimaryCrops, BankAccountNumber = user.BankAccountNumber, BankIfsc = user.BankIfsc, AccountHolderName = user.AccountHolderName, BusinessName = user.BusinessName, DeliveryAddress = user.DeliveryAddress, GstNumber = user.GstNumber, UpiId = user.UpiId
        };

        private static OrderResponseDto MapOrder(Order order) => new()
        {
            Id = order.Id, BuyerId = order.BuyerId, BuyerName = order.Buyer?.Name ?? string.Empty, BuyerPhone = order.Buyer?.Phone, IsBulkOrder = order.IsBulkOrder, DeliveryType = order.DeliveryType, DeliveryAddress = order.DeliveryAddress, Status = order.Status, TotalAmount = order.TotalAmount, CreatedAt = order.CreatedAt, UpdatedAt = order.UpdatedAt, Items = order.Items.Select(item => new OrderItemResponseDto { Id = item.Id, ProductId = item.ProductId, CropName = item.Product?.CropName ?? string.Empty, FarmerId = item.FarmerId, FarmerName = item.Farmer?.Name ?? string.Empty, Quantity = item.Quantity, PriceAtOrderTime = item.PriceAtOrderTime, SubTotal = item.SubTotal }).ToList()
        };

        private async Task<(double Lat, double Lng)?> GeocodeAddressAsync(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FasalConnect/1.0 admin-setup");

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
