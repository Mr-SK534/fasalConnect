// backend/FarmerMarketplace.Api/Utils/GeoUtils.cs

namespace FarmerMarketplace.Api.Utils
{
    public static class GeoUtils
    {
        private const double EarthRadiusKm = 6371.0;

        /// <summary>
        /// Computes the straight-line Haversine distance in meters between two (lat, lng) pairs.
        /// Returns long as OR-Tools requires integer arc costs.
        /// </summary>
        public static long DistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double rLat1 = ToRadians(lat1);
            double rLat2 = ToRadians(lat2);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(rLat1) * Math.Cos(rLat2) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            double distanceKm = EarthRadiusKm * c;
            return (long)Math.Round(distanceKm * 1000.0);
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
