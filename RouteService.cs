using System.Text.Json;
using LogiTech.Models;

namespace LogiTech.Services
{
    public class RouteService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        // ── Change these to your actual depot coordinates ──
        public const double DepotLat = -29.7235;
        public const double DepotLon = 31.0937;

        public RouteService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["ApiKeys:OpenRouteService"] ?? "";
        }

        // Convert address → lat/lon using OpenRouteService geocoding
        public async Task<(double lat, double lon)> GeocodeAddress(string address)
        {
            try
            {
                var encoded = Uri.EscapeDataString(address);
                var url = $"https://api.openrouteservice.org/geocode/search" +
                          $"?api_key={_apiKey}&text={encoded}&size=1";

                var res = await _http.GetAsync(url);
                var json = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                var coords = doc.RootElement
                    .GetProperty("features")[0]
                    .GetProperty("geometry")
                    .GetProperty("coordinates");

                // ORS returns [longitude, latitude]
                return (coords[1].GetDouble(), coords[0].GetDouble());
            }
            catch
            {
                return (0, 0);
            }
        }

        // Get real driving distance in km between two points
        public async Task<double> GetDrivingDistanceKm(
            double fromLat, double fromLon,
            double toLat, double toLon)
        {
            try
            {
                var url = "https://api.openrouteservice.org/v2/directions/driving-car" +
                          $"?api_key={_apiKey}" +
                          $"&start={fromLon},{fromLat}" +
                          $"&end={toLon},{toLat}";

                var res = await _http.GetAsync(url);
                var json = await res.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(json);

                double metres = doc.RootElement
                    .GetProperty("features")[0]
                    .GetProperty("properties")
                    .GetProperty("segments")[0]
                    .GetProperty("distance")
                    .GetDouble();

                return Math.Round(metres / 1000.0, 1);
            }
            catch
            {
                // Fallback to straight line if API fails
                return Math.Round(HaversineKm(fromLat, fromLon, toLat, toLon), 1);
            }
        }

        // Nearest-neighbour optimisation
        // HIGH priority always goes first, then nearest to nearest
        public async Task<List<DeliveryStop>> OptimiseRoute(
            List<DeliveryStop> stops)
        {
            if (!stops.Any()) return stops;

            var high = stops.Where(s => s.Priority == "HIGH").ToList();
            var medium = stops.Where(s => s.Priority == "MEDIUM").ToList();
            var low = stops.Where(s => s.Priority == "LOW").ToList();

            var ordered = new List<DeliveryStop>();
            double currentLat = DepotLat;
            double currentLon = DepotLon;

            // Process each priority group nearest-first
            foreach (var group in new[] { high, medium, low })
            {
                var remaining = group.ToList();
                while (remaining.Any())
                {
                    var nearest = FindNearest(remaining, currentLat, currentLon);
                    ordered.Add(nearest);
                    remaining.Remove(nearest);
                    currentLat = nearest.Latitude;
                    currentLon = nearest.Longitude;
                }
            }

            // Assign order numbers, real distances and ETAs
            var departure = DateTime.Today.AddHours(8); // 08:00 depot start
            double prevLat = DepotLat;
            double prevLon = DepotLon;

            for (int i = 0; i < ordered.Count; i++)
            {
                double dist = await GetDrivingDistanceKm(
                    prevLat, prevLon,
                    ordered[i].Latitude,
                    ordered[i].Longitude);

                ordered[i].OptimalOrder = i + 1;
                ordered[i].DistanceFromPrevKm = (decimal)dist;

                // 40 km/h average + 10 min per stop
                double travelMins = (dist / 40.0) * 60;
                departure = departure.AddMinutes(travelMins + 10);
                ordered[i].ETA = departure.ToString("HH:mm");

                prevLat = ordered[i].Latitude;
                prevLon = ordered[i].Longitude;
            }

            return ordered;
        }

        // Find nearest stop using straight-line distance (fast)
        private DeliveryStop FindNearest(
            List<DeliveryStop> stops,
            double fromLat, double fromLon)
        {
            return stops.OrderBy(s =>
                HaversineKm(fromLat, fromLon, s.Latitude, s.Longitude))
                .First();
        }

        // Straight-line distance between two coordinates
        public double HaversineKm(
            double lat1, double lon1,
            double lat2, double lon2)
        {
            const double R = 6371;
            double dLat = (lat2 - lat1) * Math.PI / 180;
            double dLon = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180) *
                       Math.Cos(lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}