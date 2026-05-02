using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogiTech.Data;
using LogiTech.Models;
using LogiTech.Services;

namespace LogiTech.Controllers
{
    public class RouteController : Controller
    {
        private readonly LogiTechDbContext _context;
        private readonly RouteService _routeService;

        public RouteController(LogiTechDbContext context, RouteService routeService)
        {
            _context = context;
            _routeService = routeService;
        }

        // GET: Route
        public async Task<IActionResult> Index()
        {
            // if (HttpContext.Session.GetString("UserRole") == null)
            //     return RedirectToAction("Login", "Auth");

            var stops = await _context.DeliveryStops
                .Where(s => !s.IsCompleted)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            if (stops.Any())
            {
                // Geocode any stops missing coordinates
                foreach (var stop in stops.Where(
                    s => s.Latitude == 0 && s.Longitude == 0))
                {
                    var (lat, lon) = await _routeService
                        .GeocodeAddress(stop.Address);
                    stop.Latitude = lat;
                    stop.Longitude = lon;
                }
                await _context.SaveChangesAsync();

                // Run optimisation algorithm
                stops = await _routeService.OptimiseRoute(stops);
            }

            // Summary stats for the view
            ViewBag.TotalDistance = stops
                .Sum(s => s.DistanceFromPrevKm);
            ViewBag.TotalStops = stops.Count;
            ViewBag.HighCount = stops.Count(s => s.Priority == "HIGH");
            ViewBag.MediumCount = stops.Count(s => s.Priority == "MEDIUM");
            ViewBag.LowCount = stops.Count(s => s.Priority == "LOW");
            ViewBag.DepotLat = RouteService.DepotLat;
            ViewBag.DepotLon = RouteService.DepotLon;

            return View(stops);
        }

        // GET: Route/AddStop
        public IActionResult AddStop()
        {
            return View();
        }

        // POST: Route/AddStop
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStop(DeliveryStop stop)
        {
            if (ModelState.IsValid)
            {
                // Geocode the address immediately on save
                var (lat, lon) = await _routeService
                    .GeocodeAddress(stop.Address);

                stop.Latitude = lat;
                stop.Longitude = lon;
                stop.CreatedAt = DateTime.Now;

                _context.DeliveryStops.Add(stop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(stop);
        }

        // POST: Route/MarkComplete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int id)
        {
            var stop = await _context.DeliveryStops.FindAsync(id);
            if (stop != null)
            {
                stop.IsCompleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Route/DeleteStop/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStop(int id)
        {
            var stop = await _context.DeliveryStops.FindAsync(id);
            if (stop != null)
            {
                _context.DeliveryStops.Remove(stop);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}