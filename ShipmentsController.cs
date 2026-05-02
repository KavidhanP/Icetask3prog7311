using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using LogiTech.Data;
using LogiTech.Models;

namespace LogiTech.Controllers
{
    public class ShipmentController : Controller
    {
        private readonly LogiTechDbContext _context;
        private readonly HttpClient _httpClient;

        public ShipmentController(LogiTechDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: Shipment
        public IActionResult Index()
        {
            /*if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Auth");*/

            var shipments = _context.Shipments.ToList();
            return View(shipments);
        }

        // GET: Shipment/Create
        public async Task<IActionResult> Create()
        { /*if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Auth");*/

            ViewBag.RecentShipments = _context.Shipments
                .OrderByDescending(s => s.CreatedAt)
                .Take(3)
                .ToList();

            // Fetch live exchange rates
            ViewBag.ExchangeRates = await GetExchangeRates();
            return View();
        }

        // POST: Shipment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shipment shipment)
        {
            if (ModelState.IsValid)
            {
                // Auto-generate tracking number if empty
                if (string.IsNullOrEmpty(shipment.TrackingNumber))
                {
                    shipment.TrackingNumber = "LOG-" +
                        new Random().Next(1000, 9999) + "-" +
                        new Random().Next(10, 99);
                }

                shipment.Status = "Pending Dispatch";
                shipment.CreatedAt = DateTime.Now;
                _context.Shipments.Add(shipment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RecentShipments = _context.Shipments
                .OrderByDescending(s => s.CreatedAt)
                .Take(3)
                .ToList();
            ViewBag.ExchangeRates = await GetExchangeRates();
            return View(shipment);
        }

        // API endpoint called by JavaScript for live conversion
        [HttpGet]
        public async Task<IActionResult> GetRates()
        {
            var rates = await GetExchangeRates();
            return Json(rates);
        }

        private async Task<Dictionary<string, decimal>> GetExchangeRates()
        {
            try
            {
                // Free API — no key needed for basic use
                var response = await _httpClient.GetAsync(
                    "https://open.er-api.com/v6/latest/ZAR");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonDocument.Parse(json);
                    var rates = data.RootElement.GetProperty("rates");

                    return new Dictionary<string, decimal>
                    {
                        ["ZAR"] = 1,
                        ["USD"] = Math.Round(1 / rates.GetProperty("USD").GetDecimal(), 4),
                        ["GBP"] = Math.Round(1 / rates.GetProperty("GBP").GetDecimal(), 4),
                        ["EUR"] = Math.Round(1 / rates.GetProperty("EUR").GetDecimal(), 4),
                        ["CAD"] = Math.Round(1 / rates.GetProperty("CAD").GetDecimal(), 4),
                    };
                }
            }
            catch { /* fall through to defaults */ }

            // Fallback if API fails
            return new Dictionary<string, decimal>
            {
                ["ZAR"] = 1,
                ["USD"] = 18.50m,
                ["GBP"] = 23.15m,
                ["EUR"] = 19.82m,
                ["CAD"] = 13.45m,
            };
        }
    }
}