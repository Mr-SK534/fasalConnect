// backend/FarmerMarketplace.Api/Records/RouteStop.cs

namespace FarmerMarketplace.Api.Records
{
    public record RouteStop(
        int Sequence,
        int VehicleNumber,
        string Type, // "pickup" or "delivery"
        string Label,
        double Lat,
        double Lng,
        Guid OrderId,
        double QuantityAtStop,
        DateTime EstimatedArrival = default,
        string PerishabilityTier = "Low",
        DateTime DeliveryDeadline = default,
        bool IsUrgent = false
    );
}
