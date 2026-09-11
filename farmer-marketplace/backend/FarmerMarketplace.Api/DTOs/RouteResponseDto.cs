// backend/FarmerMarketplace.Api/DTOs/RouteResponseDto.cs

using System;
using System.Collections.Generic;
using FarmerMarketplace.Api.Models;
using RouteStopRecord = FarmerMarketplace.Api.Records.RouteStop;

namespace FarmerMarketplace.Api.DTOs
{
    public class RouteResponseDto
    {
        public Guid Id { get; set; }

        // Backward compatibility alias for single-route optimizer
        public Guid RouteId
        {
            get => Id;
            set => Id = value;
        }

        public DateTime BatchDate { get; set; } = DateTime.UtcNow;

        // Backward compatibility alias for CreatedAt
        public DateTime CreatedAt
        {
            get => BatchDate;
            set => BatchDate = value;
        }

        public int VehicleCount { get; set; }
        public double TotalDistanceKm { get; set; }
        public DeliveryRouteStatus Status { get; set; } = DeliveryRouteStatus.Generated;

        // Fleet dispatch grouped by vehicle
        public Dictionary<int, List<RouteStopRecord>> StopsByVehicle { get; set; } = new();

        // Single route stops list (used by single-hub direct routing)
        public List<RouteStopResponseDto> Stops { get; set; } = new();

        // Optional hub coordinates (for direct routing)
        public double? DeliveryHubLat { get; set; }
        public double? DeliveryHubLng { get; set; }

        public List<Guid> SkippedOrderIds { get; set; } = new();
        public bool HasUrgentStops { get; set; }
        public string? WarningMessage { get; set; }
    }
}