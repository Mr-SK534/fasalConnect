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
        private readonly IPlatformConfigService _configService;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            AppDbContext context,
            IHttpClientFactory httpClientFactory,
            IPlatformConfigService configService,
            ILogger<OrderService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configService = configService;
            _logger = logger;
        }

        public async Task<OrderResponseDto> CreateAsync(Guid buyerId, OrderDto dto)
        {
            if (dto.DeliveryType == DeliveryType.Delivery && string.IsNullOrWhiteSpace(dto.DeliveryAddress))
                throw new ArgumentException("Delivery address is required for delivery orders.");

            var buyerExists = await _context.Users.AnyAsync(u => u.Id == buyerId);
            if (!buyerExists)
                throw new KeyNotFoundException("Buyer account not found.");

            var minQty = await _configService.GetDoubleAsync("min_order_quantity_kg", 1.0);
            var maxQty = await _configService.GetDoubleAsync("max_order_quantity_kg", 10000.0);

            var totalRequestedKg = (double)dto.Items.Sum(i => i.Quantity);
            if (totalRequestedKg < minQty)
                throw new InvalidOperationException($"Order quantity ({totalRequestedKg} kg) is below minimum required limit ({minQty} kg).");
            if (totalRequestedKg > maxQty)
                throw new InvalidOperationException($"Order quantity ({totalRequestedKg} kg) exceeds maximum allowed limit ({maxQty} kg).");

            var order = new Order
            {
                BuyerId = buyerId,
                IsBulkOrder = dto.IsBulkOrder,
                DeliveryType = dto.DeliveryType,
                DeliveryAddress = dto.DeliveryType == DeliveryType.Delivery ? dto.DeliveryAddress : null,
                Status = OrderStatus.Pending,
                QuantityOrderedKg = totalRequestedKg
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

                var farmerPricePerKg = FarmerMarketplace.Api.Helpers.UnitConverter.ToPricePerKg(product.Price, product.Unit);
                if (string.IsNullOrWhiteSpace(order.CropName)) order.CropName = product.CropName;
                if (!order.FarmerAskingPricePerKg.HasValue) order.FarmerAskingPricePerKg = farmerPricePerKg;
                if (!order.FarmerId.HasValue) order.FarmerId = product.FarmerId;
                if (!order.FpoAdminId.HasValue && product.Farmer?.FpoId.HasValue == true) order.FpoAdminId = product.Farmer.FpoId;

                if (!order.PickupLat.HasValue && product.Farmer?.Latitude.HasValue == true)
                {
                    order.PickupLat = product.Farmer.Latitude;
                    order.PickupLng = product.Farmer.Longitude;
                }

                var buyerPrice = await _configService.CalculateBuyerPriceAsync(farmerPricePerKg, product.CropName);

                var remaining = itemDto.Quantity;
                var productQtyInKg = FarmerMarketplace.Api.Helpers.UnitConverter.ToKgQuantity(product.Quantity, product.Unit);
                var originalProductQuantityKg = productQtyInKg;

                // Try to fulfill from the requested farmer's own stock first
                var fromThisFarmer = Math.Min(remaining, productQtyInKg);

                if (fromThisFarmer > 0)
                {
                    var subTotal = fromThisFarmer * buyerPrice;
                    order.Items.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        FarmerId = product.FarmerId,
                        Quantity = fromThisFarmer,
                        PriceAtOrderTime = buyerPrice,
                        SubTotal = subTotal
                    });

                    var takenInOriginalUnit = FarmerMarketplace.Api.Helpers.UnitConverter.ToOriginalUnitQuantity(fromThisFarmer, product.Unit);
                    product.Quantity -= takenInOriginalUnit;
                    if (product.Quantity <= 0) product.IsActive = false;

                    totalAmount += subTotal;
                    remaining -= fromThisFarmer;
                }

                // Not enough stock from this farmer alone
                if (remaining > 0)
                {
                    if (!dto.IsBulkOrder)
                        throw new InvalidOperationException(
                            $"Insufficient stock. This farmer has {originalProductQuantityKg} Kg available. Enable bulk order to source from multiple farmers.");

                    var otherSuppliers = await _context.Products
                        .Include(p => p.Farmer)
                        .Where(p => p.IsActive && p.Quantity > 0
                                    && p.Id != product.Id
                                    && p.FarmerId != product.FarmerId
                                    && p.CropName.ToLower() == product.CropName.ToLower())
                        .ToListAsync();

                    var orderedSuppliers = otherSuppliers
                        .OrderByDescending(p => FarmerMarketplace.Api.Helpers.UnitConverter.ToKgQuantity(p.Quantity, p.Unit))
                        .ToList();

                    foreach (var supplier in orderedSuppliers)
                    {
                        if (remaining <= 0) break;

                        var supplierQtyInKg = FarmerMarketplace.Api.Helpers.UnitConverter.ToKgQuantity(supplier.Quantity, supplier.Unit);
                        var take = Math.Min(remaining, supplierQtyInKg);
                        if (take <= 0) continue;

                        var supplierFarmerPricePerKg = FarmerMarketplace.Api.Helpers.UnitConverter.ToPricePerKg(supplier.Price, supplier.Unit);
                        var supplierBuyerPrice = await _configService.CalculateBuyerPriceAsync(supplierFarmerPricePerKg, supplier.CropName);
                        var subTotal = take * supplierBuyerPrice;

                        order.Items.Add(new OrderItem
                        {
                            ProductId = supplier.Id,
                            FarmerId = supplier.FarmerId,
                            Quantity = take,
                            PriceAtOrderTime = supplierBuyerPrice,
                            SubTotal = subTotal
                        });

                        var supplierTakenInOriginalUnit = FarmerMarketplace.Api.Helpers.UnitConverter.ToOriginalUnitQuantity(take, supplier.Unit);
                        supplier.Quantity -= supplierTakenInOriginalUnit;
                        if (supplier.Quantity <= 0) supplier.IsActive = false;

                        totalAmount += subTotal;
                        remaining -= take;
                    }

                    if (remaining > 0)
                    {
                        var totalAvailableKg = originalProductQuantityKg + orderedSuppliers.Sum(supplier => FarmerMarketplace.Api.Helpers.UnitConverter.ToKgQuantity(supplier.Quantity, supplier.Unit));
                        var farmerCount = orderedSuppliers.Select(supplier => supplier.FarmerId).Append(product.FarmerId).Distinct().Count();
                        throw new InvalidOperationException(
                            $"Insufficient stock. Total available: {totalAvailableKg} Kg across {farmerCount} farmers");
                    }
                }
            }

            order.TotalAmount = totalAmount;
            order.Season = string.IsNullOrWhiteSpace(order.Season) ? "S1 - Rabi Glut (Jan-Apr)" : order.Season;
            order.DeliveryDateTarget = DateTime.UtcNow.AddDays(2);
            order.DeliveryLat = order.Latitude;
            order.DeliveryLng = order.Longitude;

            _context.Orders.Add(order);

            var farmerAskingPrice = order.FarmerAskingPricePerKg ?? 20.0m;
            var farmerGross = farmerAskingPrice * (decimal)totalRequestedKg;

            var escrow = new EscrowTransaction
            {
                OrderId = order.Id,
                BuyerId = buyerId,
                PlatformAccountId = "PLATFORM_ESCROW_WALLET_01",
                OrderedAmountRs = totalAmount,
                ActualAmountRs = farmerGross,
                Status = EscrowStatus.Held,
                HeldDate = DateTime.UtcNow
            };
            _context.EscrowTransactions.Add(escrow);

            // Sync order items to SalesHistories for real-time AI Demand Forecasting
            foreach (var orderItem in order.Items)
            {
                var prod = await _context.Products
                    .Include(p => p.Farmer)
                    .FirstOrDefaultAsync(p => p.Id == orderItem.ProductId);

                string crop = prod?.CropName ?? order.CropName ?? "Produce";
                string category = prod?.Category.ToString() ?? "Vegetables";
                string region = prod?.Farmer?.District ?? prod?.Farmer?.Location ?? order.DeliveryAddress ?? "Nashik";

                _context.SalesHistories.Add(new SalesHistory
                {
                    Id = Guid.NewGuid(),
                    CropName = crop,
                    Category = category,
                    Region = region,
                    Date = DateTime.UtcNow.Date,
                    QuantitySoldKg = (float)orderItem.Quantity,
                    AveragePricePerKg = (float)orderItem.PriceAtOrderTime,
                    CreatedAt = DateTime.UtcNow
                });
            }

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
                    {
                        return (latitude, longitude);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Geocoding attempt failed for address candidate: {Candidate}", candidate);
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
            bool isAdminRole = !string.IsNullOrEmpty(role) && (
                role.Equals("PlatformAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Manager", StringComparison.OrdinalIgnoreCase)
            );

            if (!isBuyer && !isInvolvedFarmer && !isAdminRole)
                throw new UnauthorizedAccessException("You do not have access to this order.");

            return MapToResponseDto(order, farmerScopedTo: null);
        }

        public async Task<List<OrderResponseDto>> GetByBuyerIdAsync(Guid buyerId, Guid requestingUserId, string? role)
        {
            bool isAdminRole = !string.IsNullOrEmpty(role) && (
                role.Equals("PlatformAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Manager", StringComparison.OrdinalIgnoreCase)
            );

            if (buyerId != requestingUserId && !isAdminRole)
            {
                buyerId = requestingUserId;
            }

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => o.BuyerId == buyerId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new Order
                {
                    Id = o.Id,
                    BuyerId = o.BuyerId,
                    Buyer = o.Buyer,
                    IsBulkOrder = o.IsBulkOrder,
                    DeliveryType = o.DeliveryType,
                    DeliveryAddress = o.DeliveryAddress,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    CropName = o.CropName,
                    Season = o.Season,
                    QuantityOrderedKg = o.QuantityOrderedKg,
                    QuantityPickedUpKg = o.QuantityPickedUpKg,
                    QuantityDeliveredKg = o.QuantityDeliveredKg,
                    FarmerAskingPricePerKg = o.FarmerAskingPricePerKg,
                    FarmerId = o.FarmerId,
                    FpoAdminId = o.FpoAdminId,
                    Latitude = o.Latitude,
                    Longitude = o.Longitude,
                    DeliveryLat = o.DeliveryLat,
                    DeliveryLng = o.DeliveryLng,
                    PickupLat = o.PickupLat,
                    PickupLng = o.PickupLng,
                    DeliveryDateTarget = o.DeliveryDateTarget,
                    DeliveryConfirmedDate = o.DeliveryConfirmedDate,
                    RouteId = o.RouteId,
                    StopSequence = o.StopSequence,
                    VehicleNumber = o.VehicleNumber,
                    EstimatedArrival = o.EstimatedArrival,
                    Items = o.Items.Select(i => new OrderItem
                    {
                        Id = i.Id,
                        OrderId = i.OrderId,
                        ProductId = i.ProductId,
                        FarmerId = i.FarmerId,
                        Quantity = i.Quantity,
                        PriceAtOrderTime = i.PriceAtOrderTime,
                        SubTotal = i.SubTotal,
                        Farmer = i.Farmer,
                        Product = i.Product == null ? null : new Product
                        {
                            Id = i.Product.Id,
                            CropName = i.Product.CropName,
                            Price = i.Product.Price,
                            Unit = i.Product.Unit,
                            Category = i.Product.Category
                        }
                    }).ToList()
                })
                .ToListAsync();

            return orders.Select(o => MapToResponseDto(o, farmerScopedTo: null)).ToList();
        }

        public async Task<List<OrderResponseDto>> GetByFarmerIdAsync(Guid farmerId, Guid requestingUserId, string? role)
        {
            bool isAdminRole = !string.IsNullOrEmpty(role) && (
                role.Equals("PlatformAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Manager", StringComparison.OrdinalIgnoreCase)
            );
            bool isFpoAdmin = !string.IsNullOrEmpty(role)
                && role.Equals("FpoAdmin", StringComparison.OrdinalIgnoreCase)
                && await _context.Users.AnyAsync(user => user.Id == farmerId && user.FpoId == requestingUserId);

            // If requested farmerId differs from logged-in userId and user is not an admin/FPO admin,
            // fall back to requestingUserId so logged-in farmer always views their own orders safely.
            if (farmerId != requestingUserId && !isAdminRole && !isFpoAdmin)
            {
                farmerId = requestingUserId;
            }

            var orderIds = await _context.OrderItems
                .Where(i => i.FarmerId == farmerId)
                .Select(i => i.OrderId)
                .Distinct()
                .ToListAsync();

            var orders = await _context.Orders
                .AsNoTracking()
                .Where(o => orderIds.Contains(o.Id))
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new Order
                {
                    Id = o.Id,
                    BuyerId = o.BuyerId,
                    Buyer = o.Buyer,
                    IsBulkOrder = o.IsBulkOrder,
                    DeliveryType = o.DeliveryType,
                    DeliveryAddress = o.DeliveryAddress,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    CropName = o.CropName,
                    Season = o.Season,
                    QuantityOrderedKg = o.QuantityOrderedKg,
                    QuantityPickedUpKg = o.QuantityPickedUpKg,
                    QuantityDeliveredKg = o.QuantityDeliveredKg,
                    FarmerAskingPricePerKg = o.FarmerAskingPricePerKg,
                    FarmerId = o.FarmerId,
                    FpoAdminId = o.FpoAdminId,
                    Latitude = o.Latitude,
                    Longitude = o.Longitude,
                    DeliveryLat = o.DeliveryLat,
                    DeliveryLng = o.DeliveryLng,
                    PickupLat = o.PickupLat,
                    PickupLng = o.PickupLng,
                    DeliveryDateTarget = o.DeliveryDateTarget,
                    DeliveryConfirmedDate = o.DeliveryConfirmedDate,
                    RouteId = o.RouteId,
                    StopSequence = o.StopSequence,
                    VehicleNumber = o.VehicleNumber,
                    EstimatedArrival = o.EstimatedArrival,
                    Items = o.Items.Select(i => new OrderItem
                    {
                        Id = i.Id,
                        OrderId = i.OrderId,
                        ProductId = i.ProductId,
                        FarmerId = i.FarmerId,
                        Quantity = i.Quantity,
                        PriceAtOrderTime = i.PriceAtOrderTime,
                        SubTotal = i.SubTotal,
                        Farmer = i.Farmer,
                        Product = i.Product == null ? null : new Product
                        {
                            Id = i.Product.Id,
                            CropName = i.Product.CropName,
                            Price = i.Product.Price,
                            Unit = i.Product.Unit,
                            Category = i.Product.Category
                        }
                    }).ToList()
                })
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
            bool isAdminRole = !string.IsNullOrEmpty(role) && (
                role.Equals("PlatformAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("Manager", StringComparison.OrdinalIgnoreCase)
            );
            var isFpoAdmin = false;

            if (!string.IsNullOrEmpty(role) && role.Equals("FpoAdmin", StringComparison.OrdinalIgnoreCase))
            {
                var farmerIds = order.Items.Select(item => item.FarmerId).Distinct().ToList();
                isFpoAdmin = await _context.Users.AnyAsync(user => user.Id == requestingUserId && user.Role == UserRole.FpoAdmin)
                    && await _context.Users.AnyAsync(user => farmerIds.Contains(user.Id) && user.FpoId == requestingUserId);
            }

            if (!isInvolvedFarmer && !isAdminRole && !isFpoAdmin)
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

            if (dto.Status == OrderStatus.Delivered)
            {
                order.DeliveryConfirmedDate ??= DateTime.UtcNow;
                var deliveredKg = order.QuantityDeliveredKg ?? order.QuantityOrderedKg ?? (order.Items.Any() ? (double)order.Items.Sum(i => i.Quantity) : 1.0);
                order.QuantityDeliveredKg = deliveredKg;

                var escrow = await _context.EscrowTransactions.FirstOrDefaultAsync(e => e.OrderId == order.Id);
                if (escrow != null)
                {
                    escrow.Status = EscrowStatus.Released;
                    escrow.ReleaseDate = DateTime.UtcNow;
                }

                // Auto-create payout and transaction ledger entries for linked farmers
                var farmerId = order.FarmerId ?? order.Items.FirstOrDefault()?.FarmerId;
                if (farmerId.HasValue)
                {
                    var farmer = await _context.Users.FirstOrDefaultAsync(u => u.Id == farmerId.Value);
                    var price = order.FarmerAskingPricePerKg ?? (order.Items.FirstOrDefault()?.PriceAtOrderTime ?? 20.0m);
                    var farmerEarn = price * (decimal)deliveredKg;

                    var orderIdStr = order.Id.ToString();
                    var existingPayout = await _context.FarmerPayouts.FirstOrDefaultAsync(p => p.OrderIdsJson.Contains(orderIdStr));
                    if (existingPayout == null && farmer != null)
                    {
                        _context.FarmerPayouts.Add(new FarmerPayout
                        {
                            FarmerId = farmer.Id,
                            FpoAdminId = farmer.FpoId,
                            OrderIdsJson = System.Text.Json.JsonSerializer.Serialize(new List<Guid> { order.Id }),
                            TotalAmountRs = farmerEarn,
                            PayoutDate = DateTime.UtcNow,
                            PaymentMethod = !string.IsNullOrWhiteSpace(farmer.UpiId) ? PayoutPaymentMethod.Upi : PayoutPaymentMethod.BankTransfer,
                            UpiIdOrBankAccount = farmer.BankAccountNumber ?? farmer.UpiId ?? "Bank Account",
                            Status = PayoutStatus.Completed,
                            ConfirmationTimestamp = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        });

                        _context.TransactionLedgers.Add(new TransactionLedger
                        {
                            OrderId = order.Id,
                            TransactionType = LedgerTransactionType.PayoutToFarmer,
                            FromAccount = "platform_escrow",
                            ToAccount = $"farmer_bank:{farmer.BankAccountNumber ?? farmer.UpiId ?? farmer.Phone}",
                            AmountRs = farmerEarn,
                            Status = LedgerStatus.Completed,
                            Notes = $"100% asking price payout transferred directly to farmer bank account ({farmer.BankAccountNumber ?? farmer.UpiId ?? "Bank Account"}).",
                            CreatedBy = "order_delivery_sync"
                        });
                    }
                }
            }

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

            var primaryFarmer = order.Farmer ?? items.FirstOrDefault()?.Farmer;

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
                TotalAmount = farmerScopedTo.HasValue ? itemDtos.Sum(i => i.SubTotal) : order.TotalAmount,
                Items = itemDtos,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,

                CropName = order.CropName ?? itemDtos.FirstOrDefault()?.CropName ?? "Produce",
                Season = order.Season ?? "S1 - Rabi Glut (Jan-Apr)",
                QuantityOrderedKg = order.QuantityOrderedKg ?? (double)itemDtos.Sum(i => i.Quantity),
                QuantityPickedUpKg = order.QuantityPickedUpKg,
                QuantityDeliveredKg = order.QuantityDeliveredKg,
                FarmerAskingPricePerKg = order.FarmerAskingPricePerKg ?? itemDtos.FirstOrDefault()?.PriceAtOrderTime ?? 20.0m,
                FarmerId = order.FarmerId ?? primaryFarmer?.Id,
                FarmerName = primaryFarmer?.Name ?? itemDtos.FirstOrDefault()?.FarmerName ?? string.Empty,
                FpoAdminId = order.FpoAdminId ?? primaryFarmer?.FpoId,
                Latitude = order.Latitude,
                Longitude = order.Longitude,
                DeliveryLat = order.DeliveryLat ?? order.Latitude,
                DeliveryLng = order.DeliveryLng ?? order.Longitude,
                PickupLat = order.PickupLat ?? primaryFarmer?.Latitude,
                PickupLng = order.PickupLng ?? primaryFarmer?.Longitude,
                DeliveryDateTarget = order.DeliveryDateTarget ?? order.CreatedAt.AddDays(2),
                DeliveryConfirmedDate = order.DeliveryConfirmedDate,
                RouteId = order.RouteId,
                StopSequence = order.StopSequence,
                VehicleNumber = order.VehicleNumber,
                EstimatedArrival = order.EstimatedArrival
            };
        }
    }
}