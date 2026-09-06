// backend/FarmerMarketplace.Api/Services/OrderService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FarmerMarketplace.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderService> _logger;

        public OrderService(AppDbContext context, IHttpClientFactory httpClientFactory, ILogger<OrderService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<OrderResponseDto> CreateAsync(Guid buyerId, OrderDto dto)
        {
            if (dto.DeliveryType == DeliveryType.Delivery && string.IsNullOrWhiteSpace(dto.DeliveryAddress))
                throw new ArgumentException("Delivery address is required for delivery orders.");

            var buyerExists = await _context.Users.AnyAsync(u => u.Id == buyerId);
            if (!buyerExists)
                throw new KeyNotFoundException("Buyer account not found.");

            var order = new Order
            {
                BuyerId = buyerId,
                IsBulkOrder = dto.IsBulkOrder,
                DeliveryType = dto.DeliveryType,
                DeliveryAddress = dto.DeliveryType == DeliveryType.Delivery ? dto.DeliveryAddress : null,
                Status = OrderStatus.Pending
            };

            if (order.DeliveryType == DeliveryType.Delivery)
            {
                var coordinates = await GeocodeAddressAsync(order.DeliveryAddress);
                if (coordinates.HasValue)
                {
                    order.Latitude = coordinates.Value.Lat;
                    order.Longitude = coordinates.Value.Lng;
                }
                else
                {
                    _logger.LogWarning("Could not geocode delivery address during order creation: {Address}", order.DeliveryAddress);
                }
            }

            decimal totalAmount = 0;

            foreach (var itemDto in dto.Items)
            {
                var product = await _context.Products
                    .Include(p => p.Farmer)
                    .FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);

                if (product == null)
                    throw new KeyNotFoundException("Product is no longer available.");

                if (!product.IsActive)
                    throw new KeyNotFoundException($"Product '{product.CropName}' is no longer available.");

                if (product.Quantity <= 0)
                    throw new InvalidOperationException($"Product '{product.CropName}' is out of stock.");

                var remaining = itemDto.Quantity;
                var originalProductQuantity = product.Quantity;

                // Try to fulfill from the requested farmer's own stock first
                var fromThisFarmer = Math.Min(remaining, product.Quantity);

                if (fromThisFarmer > 0)
                {
                    var subTotal = fromThisFarmer * product.Price;
                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        FarmerId = product.FarmerId,
                        Quantity = fromThisFarmer,
                        PriceAtOrderTime = product.Price,
                        SubTotal = subTotal
                    });

                    product.Quantity -= fromThisFarmer;
                    if (product.Quantity <= 0) product.IsActive = false;

                    totalAmount += subTotal;
                    remaining -= fromThisFarmer;
                }

                // Not enough stock from this farmer alone
                if (remaining > 0)
                {
                    if (!dto.IsBulkOrder)
                        throw new InvalidOperationException(
                            $"Insufficient stock. This farmer has {product.Quantity + fromThisFarmer} {product.Unit}. Enable bulk order to source from multiple farmers.");

                    // Order Aggregator: split the remainder across other farmers
                    // listing the same crop, largest stock first
                    var otherSuppliers = await _context.Products
                        .Include(p => p.Farmer)
                        .Where(p => p.IsActive && p.Quantity > 0
                                    && p.Id != product.Id
                                    && p.FarmerId != product.FarmerId
                                    && p.CropName.ToLower() == product.CropName.ToLower())
                        .OrderByDescending(p => p.Quantity)
                        .ToListAsync();

                    foreach (var supplier in otherSuppliers)
                    {
                        if (remaining <= 0) break;

                        var take = Math.Min(remaining, supplier.Quantity);
                        if (take <= 0) continue;

                        var subTotal = take * supplier.Price;
                        order.Items.Add(new OrderItem
                        {
                            ProductId = supplier.Id,
                            FarmerId = supplier.FarmerId,
                            Quantity = take,
                            PriceAtOrderTime = supplier.Price,
                            SubTotal = subTotal
                        });

                        supplier.Quantity -= take;
                        if (supplier.Quantity <= 0) supplier.IsActive = false;

                        totalAmount += subTotal;
                        remaining -= take;
                    }

                    if (remaining > 0)
                    {
                        var totalAvailable = originalProductQuantity + otherSuppliers.Sum(supplier => supplier.Quantity);
                        var farmerCount = otherSuppliers.Select(supplier => supplier.FarmerId).Append(product.FarmerId).Distinct().Count();
                        throw new InvalidOperationException(
                            $"Insufficient stock. Total available: {totalAvailable} {product.Unit} across {farmerCount} farmers");
                    }
                }
            }

            order.TotalAmount = totalAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(order.Id, buyerId, nameof(UserRole.Buyer));
        }

        private async Task<(double Lat, double Lng)?> GeocodeAddressAsync(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return null;

            var normalized = address.Trim();
            var postalCode = Regex.Match(normalized, @"\b\d{6}\b").Value;
            var locality = Regex.Replace(normalized, @"^(flat|floor|house|plot|door|unit)\s+[^,]+,?\s*", string.Empty, RegexOptions.IgnoreCase).Trim();
            var candidates = new[] { normalized, $"{normalized}, India", locality, $"{locality}, India", string.IsNullOrWhiteSpace(postalCode) ? null : $"{postalCode}, India" }
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate))
                .Select(candidate => candidate!)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FasalConnect/1.0 route-planner");

            foreach (var candidate in candidates)
            {
                try
                {
                    var url = $"https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&countrycodes=in&q={Uri.EscapeDataString(candidate)}";
                    using var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) continue;
                    using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    var result = document.RootElement.EnumerateArray().FirstOrDefault();
                    if (result.ValueKind == JsonValueKind.Undefined) continue;
                    var lat = result.TryGetProperty("lat", out var latProperty) ? latProperty.GetString() : null;
                    var lng = result.TryGetProperty("lon", out var lngProperty) ? lngProperty.GetString() : null;
                    if (double.TryParse(lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
                        && double.TryParse(lng, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
                        return (latitude, longitude);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Geocoding candidate failed: {Address}", candidate);
                }
            }

            return null;
        }

        public async Task<OrderResponseDto> GetByIdAsync(Guid id, Guid requestingUserId, string? role)
        {
            var order = await LoadFullOrder(id);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            var isBuyer = order.BuyerId == requestingUserId;
            var isInvolvedFarmer = order.Items.Any(i => i.FarmerId == requestingUserId);
            var isPlatformAdmin = role == nameof(UserRole.PlatformAdmin);

            if (!isBuyer && !isInvolvedFarmer && !isPlatformAdmin)
                throw new UnauthorizedAccessException("You do not have access to this order.");

            return MapToResponseDto(order, farmerScopedTo: null);
        }

        public async Task<List<OrderResponseDto>> GetByBuyerIdAsync(Guid buyerId, Guid requestingUserId, string? role)
        {
            var isPlatformAdmin = role == nameof(UserRole.PlatformAdmin);

            if (buyerId != requestingUserId && !isPlatformAdmin)
                throw new UnauthorizedAccessException("You can only view your own orders.");

            var orders = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.Items).ThenInclude(i => i.Farmer)
                .Where(o => o.BuyerId == buyerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(o => MapToResponseDto(o, farmerScopedTo: null)).ToList();
        }

        public async Task<List<OrderResponseDto>> GetByFarmerIdAsync(Guid farmerId, Guid requestingUserId, string? role)
        {
            var isPlatformAdmin = role == nameof(UserRole.PlatformAdmin);
            var isFpoAdmin = role == nameof(UserRole.FpoAdmin)
                && await _context.Users.AnyAsync(user => user.Id == farmerId && user.FpoId == requestingUserId && user.Role == UserRole.Farmer);

            if (farmerId != requestingUserId && !isPlatformAdmin && !isFpoAdmin)
                throw new UnauthorizedAccessException("You can only view your own orders.");

            var orderIds = await _context.OrderItems
                .Where(i => i.FarmerId == farmerId)
                .Select(i => i.OrderId)
                .Distinct()
                .ToListAsync();

            var orders = await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.Items).ThenInclude(i => i.Farmer)
                .Where(o => orderIds.Contains(o.Id))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Scope each order's Items to just this farmer's own lines, per contract
            return orders.Select(o => MapToResponseDto(o, farmerScopedTo: farmerId)).ToList();
        }

        public async Task<OrderResponseDto> UpdateStatusAsync(Guid id, Guid requestingUserId, string? role, OrderStatusUpdateDto dto)
        {
            var order = await LoadFullOrder(id);

            if (order == null)
                throw new KeyNotFoundException("Order not found.");

            var isInvolvedFarmer = order.Items.Any(i => i.FarmerId == requestingUserId);
            var isPlatformAdmin = role == nameof(UserRole.PlatformAdmin);
            var isFpoAdmin = false;

            if (role == nameof(UserRole.FpoAdmin))
            {
                var farmerIds = order.Items.Select(item => item.FarmerId).Distinct().ToList();
                isFpoAdmin = await _context.Users.AnyAsync(user => user.Id == requestingUserId && user.Role == UserRole.FpoAdmin)
                    && await _context.Users.AnyAsync(user => farmerIds.Contains(user.Id) && user.FpoId == requestingUserId);
            }

            if (!isInvolvedFarmer && !isPlatformAdmin && !isFpoAdmin)
                throw new UnauthorizedAccessException("Only a farmer fulfilling this order or an admin can update its status.");

            var validTransition = (order.Status, dto.Status) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.InTransit) => true,
                (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
                (OrderStatus.InTransit, OrderStatus.Delivered) => true,
                _ => false
            };

            if (!validTransition)
                throw new InvalidOperationException($"Cannot change order status from {order.Status} to {dto.Status}.");

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // TODO: trigger WhatsApp notification to buyer once WhatsAppService exists
            // await _whatsAppService.NotifyOrderStatusChange(order.BuyerId, order.Id, order.Status);

            return MapToResponseDto(order, farmerScopedTo: null);
        }

        private async Task<Order?> LoadFullOrder(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Include(o => o.Items).ThenInclude(i => i.Farmer)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        private static OrderResponseDto MapToResponseDto(Order order, Guid? farmerScopedTo)
        {
            var items = farmerScopedTo.HasValue
                ? order.Items.Where(i => i.FarmerId == farmerScopedTo.Value)
                : order.Items;

            var itemDtos = items.Select(i => new OrderItemResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                CropName = i.Product?.CropName ?? string.Empty,
                FarmerId = i.FarmerId,
                FarmerName = i.Farmer?.Name ?? string.Empty,
                Quantity = i.Quantity,
                PriceAtOrderTime = i.PriceAtOrderTime,
                SubTotal = i.SubTotal
            }).ToList();

            return new OrderResponseDto
            {
                Id = order.Id,
                BuyerId = order.BuyerId,
                BuyerName = order.Buyer?.Name ?? string.Empty,
                BuyerPhone = order.Buyer?.Phone,
                IsBulkOrder = order.IsBulkOrder,
                DeliveryType = order.DeliveryType,
                DeliveryAddress = order.DeliveryAddress,
                Status = order.Status,
                // When scoped to one farmer, total reflects only their share of the order
                TotalAmount = farmerScopedTo.HasValue ? itemDtos.Sum(i => i.SubTotal) : order.TotalAmount,
                Items = itemDtos,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }
    }
}