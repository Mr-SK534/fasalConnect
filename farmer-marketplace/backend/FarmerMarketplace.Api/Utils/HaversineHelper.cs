// backend/FarmerMarketplace.Api/Utils/HaversineHelper.cs

// backend/FarmerMarketplace.Api/Utils/HaversineHelper.cs

namespace FarmerMarketplace.Api.Utils
{
    public static class HaversineHelper
    {
        private const double EarthRadiusKm = 6371.0;

        public static double DistanceKm(double lat1, double lng1, double lat2, double lng2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLng = ToRadians(lng2 - lng1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        // Assumed average delivery speed for ETA estimation
        private const double AverageSpeedKmh = 30.0;

        public static TimeSpan EstimateTravelTime(double distanceKm)
        {
            var hours = distanceKm / AverageSpeedKmh;
            return TimeSpan.FromHours(hours);
        }
    }
}