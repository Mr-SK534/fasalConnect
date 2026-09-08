// backend/FarmerMarketplace.Api/Services/PlatformConfigService.cs

using System.Collections.Concurrent;
using System.Globalization;
using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class PlatformConfigService : IPlatformConfigService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PlatformConfigService> _logger;

        private static readonly ConcurrentDictionary<string, (string RawValue, ConfigValueType Type)> _cache = new();
        private static DateTime _lastLoaded = DateTime.MinValue;
        private static readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);

        private static readonly Dictionary<string, int> RoleHierarchy = new(StringComparer.OrdinalIgnoreCase)
        {
            { "superadmin", 3 },
            { "platformadmin", 3 },
            { "admin", 2 },
            { "fpoadmin", 2 },
            { "manager", 1 },
            { "farmer", 0 },
            { "buyer", 0 }
        };

        public PlatformConfigService(IServiceScopeFactory scopeFactory, ILogger<PlatformConfigService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        private async Task EnsureLoadedAsync()
        {
            if (!_cache.IsEmpty && (DateTime.UtcNow - _lastLoaded) < _cacheTtl)
            {
                return;
            }

            await ReloadAsync();
        }

        public async Task ReloadAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var configs = await context.PlatformConfigs
                    .AsNoTracking()
                    .Where(c => c.IsActive)
                    .ToListAsync();

                _cache.Clear();
                foreach (var cfg in configs)
                {
                    _cache[cfg.Key] = (cfg.Value, cfg.ValueType);
                }

                _lastLoaded = DateTime.UtcNow;
                _logger.LogInformation("✓ Platform config loaded successfully: {Count} items.", _cache.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "✗ Failed to load platform config from database: {Message}", ex.Message);
            }
        }

        public async Task<decimal> GetDecimalAsync(string key, decimal defaultValue)
        {
            await EnsureLoadedAsync();
            if (_cache.TryGetValue(key, out var item) && decimal.TryParse(item.RawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            _logger.LogWarning("Config key '{Key}' not found in cache/DB. Using default: {Default}", key, defaultValue);
            return defaultValue;
        }

        public async Task<double> GetDoubleAsync(string key, double defaultValue)
        {
            var decVal = await GetDecimalAsync(key, (decimal)defaultValue);
            return (double)decVal;
        }

        public async Task<int> GetIntAsync(string key, int defaultValue)
        {
            await EnsureLoadedAsync();
            if (_cache.TryGetValue(key, out var item) && int.TryParse(item.RawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }

            return defaultValue;
        }

        public async Task<bool> GetBoolAsync(string key, bool defaultValue)
        {
            await EnsureLoadedAsync();
            if (_cache.TryGetValue(key, out var item) && bool.TryParse(item.RawValue, out var parsed))
            {
                return parsed;
            }

            return defaultValue;
        }

        public async Task<string> GetStringAsync(string key, string defaultValue)
        {
            await EnsureLoadedAsync();
            if (_cache.TryGetValue(key, out var item))
            {
                return item.RawValue;
            }

            return defaultValue;
        }

        public async Task<decimal> CalculateBuyerPriceAsync(decimal farmerAskingPrice, string? cropName = null, string? season = null)
        {
            decimal commissionPct = await GetDecimalAsync("commission_pct", 0.08m);

            if (!string.IsNullOrWhiteSpace(cropName))
            {
                var cropKey = $"{cropName.Trim().ToLower()}_commission_pct";
                commissionPct = await GetDecimalAsync(cropKey, commissionPct);
            }

            decimal logisticsPartner = await GetDecimalAsync("logistics_partner_payout_per_kg", 2.0m);
            decimal logisticsMargin = await GetDecimalAsync("logistics_platform_margin_per_kg", 0.5m);

            decimal seasonMultiplier = 1.0m;
            if (!string.IsNullOrWhiteSpace(season))
            {
                var seasonKey = season.ToLower() switch
                {
                    var s when s.Contains("rabi") || s.Contains("s1") => "s1_rabi_glut_multiplier",
                    var s when s.Contains("pre-monsoon") || s.Contains("s2") => "s2_pre_monsoon_multiplier",
                    var s when s.Contains("monsoon") || s.Contains("s3") => "s3_monsoon_multiplier",
                    var s when s.Contains("kharif") || s.Contains("s4") => "s4_kharif_multiplier",
                    _ => null
                };

                if (seasonKey != null)
                {
                    seasonMultiplier = await GetDecimalAsync(seasonKey, 1.0m);
                }
            }

            decimal commissionPerKg = farmerAskingPrice * commissionPct;
            decimal logisticsPerKg = logisticsPartner + logisticsMargin;
            decimal buyerPrice = (farmerAskingPrice + commissionPerKg + logisticsPerKg) * seasonMultiplier;

            return Math.Round(buyerPrice, 2);
        }

        public async Task<PlatformConfigResponseDto> GetAllConfigAsync(string userRole)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var configs = await context.PlatformConfigs
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Category)
                .ThenBy(c => c.Key)
                .ToListAsync();

            int callerLevel = RoleHierarchy.TryGetValue(userRole, out var lvl) ? lvl : 0;

            var grouped = new Dictionary<string, List<PlatformConfigItemDto>>();
            foreach (var c in configs)
            {
                if (!grouped.ContainsKey(c.Category))
                {
                    grouped[c.Category] = new List<PlatformConfigItemDto>();
                }

                int requiredLevel = RoleHierarchy.TryGetValue(c.RequiresRole, out var reqLvl) ? reqLvl : 2;

                grouped[c.Category].Add(new PlatformConfigItemDto
                {
                    Key = c.Key,
                    Value = c.Value,
                    ValueType = c.ValueType,
                    MinValue = c.MinValue,
                    MaxValue = c.MaxValue,
                    Description = c.Description,
                    RequiresRole = c.RequiresRole,
                    CanEdit = callerLevel >= requiredLevel,
                    LastUpdatedBy = c.UpdatedBy ?? "system",
                    LastUpdatedAt = c.UpdatedAt
                });
            }

            return new PlatformConfigResponseDto
            {
                UserRole = userRole,
                Config = grouped
            };
        }

        public async Task<ConfigUpdateResultDto> UpdateConfigAsync(ConfigUpdateRequestDto dto, string userEmail, string userRole)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var config = await context.PlatformConfigs.FirstOrDefaultAsync(c => c.Key == dto.Key && c.IsActive);
            if (config == null)
            {
                throw new KeyNotFoundException($"Config key '{dto.Key}' not found.");
            }

            int callerLevel = RoleHierarchy.TryGetValue(userRole, out var lvl) ? lvl : 0;
            int requiredLevel = RoleHierarchy.TryGetValue(config.RequiresRole, out var reqLvl) ? reqLvl : 2;

            if (callerLevel < requiredLevel)
            {
                throw new UnauthorizedAccessException($"Config '{dto.Key}' requires {config.RequiresRole} role to edit. Your role: {userRole}");
            }

            // Type and bounds validation
            decimal numericVal = 0m;
            if (config.ValueType == ConfigValueType.Decimal || config.ValueType == ConfigValueType.Percent)
            {
                if (!decimal.TryParse(dto.NewValue, NumberStyles.Any, CultureInfo.InvariantCulture, out numericVal))
                {
                    throw new ArgumentException($"Invalid decimal value '{dto.NewValue}' for key '{dto.Key}'");
                }
            }
            else if (config.ValueType == ConfigValueType.Integer || config.ValueType == ConfigValueType.Days)
            {
                if (!int.TryParse(dto.NewValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var intVal))
                {
                    throw new ArgumentException($"Invalid integer value '{dto.NewValue}' for key '{dto.Key}'");
                }
                numericVal = intVal;
            }

            if (config.MinValue.HasValue && numericVal < config.MinValue.Value)
            {
                throw new ArgumentOutOfRangeException(dto.Key, $"Value {numericVal} below minimum allowed ({config.MinValue.Value})");
            }
            if (config.MaxValue.HasValue && numericVal > config.MaxValue.Value)
            {
                throw new ArgumentOutOfRangeException(dto.Key, $"Value {numericVal} exceeds maximum allowed ({config.MaxValue.Value})");
            }

            var oldValue = config.Value;
            config.Value = dto.NewValue;
            config.UpdatedBy = userEmail;
            config.UpdatedAt = DateTime.UtcNow;

            // Audit Trail
            var ledger = new TransactionLedger
            {
                TransactionType = LedgerTransactionType.ConfigChange,
                FromAccount = $"user:{userEmail}",
                ToAccount = "platform_config",
                AmountRs = 0m,
                Status = LedgerStatus.Completed,
                Notes = $"[{userRole.ToUpper()}] Updated {dto.Key} from '{oldValue}' to '{dto.NewValue}'. Reason: {dto.Description}",
                CreatedBy = userEmail,
                Timestamp = DateTime.UtcNow
            };
            context.TransactionLedgers.Add(ledger);

            await context.SaveChangesAsync();

            // Refresh memory cache
            await ReloadAsync();

            return new ConfigUpdateResultDto
            {
                Status = "updated",
                Key = dto.Key,
                OldValue = oldValue,
                NewValue = dto.NewValue,
                ChangedBy = userEmail,
                ChangedByRole = userRole,
                Timestamp = DateTime.UtcNow
            };
        }

        public async Task<List<TransactionLedger>> GetAuditHistoryAsync(int limit)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            return await context.TransactionLedgers
                .AsNoTracking()
                .Where(tl => tl.TransactionType == LedgerTransactionType.ConfigChange)
                .OrderByDescending(tl => tl.Timestamp)
                .Take(limit > 0 ? limit : 50)
                .ToListAsync();
        }

        public async Task<SimulatePriceChangeResultDto> SimulatePriceChangeAsync(SimulatePriceChangeRequestDto dto)
        {
            decimal logisticsPartner = await GetDecimalAsync("logistics_partner_payout_per_kg", 2.0m);
            decimal logisticsMargin = await GetDecimalAsync("logistics_platform_margin_per_kg", 0.5m);
            decimal logisticsTotal = logisticsPartner + logisticsMargin;

            decimal commissionPerKg = dto.FarmerPrice * dto.NewCommissionPct;
            decimal buyerPrice = Math.Round(dto.FarmerPrice + commissionPerKg + logisticsTotal, 2);
            decimal platformRevenue = Math.Round(commissionPerKg + logisticsMargin, 2);

            return new SimulatePriceChangeResultDto
            {
                FarmerPrice = dto.FarmerPrice,
                NewCommissionPct = dto.NewCommissionPct,
                BuyerPrice = buyerPrice,
                PlatformRevenuePerKg = platformRevenue,
                Message = $"Simulated buyer price with {dto.NewCommissionPct * 100}% commission"
            };
        }
    }
}
