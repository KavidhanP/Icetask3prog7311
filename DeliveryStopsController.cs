using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LogiTech.Data;
using LogiTech.Models;

namespace LogiTech.Controllers
{
    public class DeliveryStopsController : Controller
    {
        private readonly LogiTechDbContext _context;

        public DeliveryStopsController(LogiTechDbContext context)
        {
            _context = context;
        }

        // GET: DeliveryStops
        public async Task<IActionResult> Index()
        {
            return View(await _context.DeliveryStops.ToListAsync());
        }

        // GET: DeliveryStops/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryStop = await _context.DeliveryStops
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deliveryStop == null)
            {
                return NotFound();
            }

            return View(deliveryStop);
        }

        // GET: DeliveryStops/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DeliveryStops/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Order,Address,Priority,ETA,DistanceKm")] DeliveryStop deliveryStop)
        {
            if (ModelState.IsValid)
            {
                _context.Add(deliveryStop);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(deliveryStop);
        }

        // GET: DeliveryStops/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryStop = await _context.DeliveryStops.FindAsync(id);
            if (deliveryStop == null)
            {
                return NotFound();
            }
            return View(deliveryStop);
        }

        // POST: DeliveryStops/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Order,Address,Priority,ETA,DistanceKm")] DeliveryStop deliveryStop)
        {
            if (id != deliveryStop.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deliveryStop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeliveryStopExists(deliveryStop.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(deliveryStop);
        }

        // GET: DeliveryStops/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryStop = await _context.DeliveryStops
                .FirstOrDefaultAsync(m => m.Id == id);
            if (deliveryStop == null)
            {
                return NotFound();
            }

            return View(deliveryStop);
        }

        // POST: DeliveryStops/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deliveryStop = await _context.DeliveryStops.FindAsync(id);
            if (deliveryStop != null)
            {
                _context.DeliveryStops.Remove(deliveryStop);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeliveryStopExists(int id)
        {
            return _context.DeliveryStops.Any(e => e.Id == id);
        }
    }
}
