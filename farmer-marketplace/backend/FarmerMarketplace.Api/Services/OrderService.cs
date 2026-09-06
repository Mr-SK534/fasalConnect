// backend/FarmerMarketplace.Api/Services/OrderService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
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

            decimal totalAmount = 0;

            foreach (var itemDto in dto.Items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);

                if (product == null || !product.IsActive)
                    throw new KeyNotFoundException($"Product {itemDto.ProductId} not found or is no longer listed.");

                var remaining = itemDto.Quantity;

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
                            $"Insufficient stock for '{product.CropName}'. Only {product.Quantity + fromThisFarmer} available.");

                    // Order Aggregator: split the remainder across other farmers
                    // listing the same crop, largest stock first
                    var otherSuppliers = await _context.Products
                        .Where(p => p.IsActive
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
                        throw new InvalidOperationException(
                            $"Insufficient stock across all farmers for '{product.CropName}'. Short by {remaining}.");
                }
            }

            order.TotalAmount = totalAmount;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(order.Id, buyerId, nameof(UserRole.Buyer));
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

            if (farmerId != requestingUserId && !isPlatformAdmin)
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

            if (!isInvolvedFarmer && !isPlatformAdmin)
                throw new UnauthorizedAccessException("Only a farmer fulfilling this order or an admin can update its status.");

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