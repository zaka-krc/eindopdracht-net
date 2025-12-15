using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem_Web
{
    public class DeliveriesController : Controller
    {
        private readonly SuntoryDbContext _context;

        public DeliveriesController(SuntoryDbContext context)
        {
            _context = context;
        }

        // GET: Deliveries
        public async Task<IActionResult> Index()
        {
            var suntoryDbContext = _context.Deliveries.Include(d => d.Customer).Include(d => d.Supplier).Include(d => d.Vehicle);
            return View(await suntoryDbContext.ToListAsync());
        }

        // GET: Deliveries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var delivery = await _context.Deliveries
                .Include(d => d.Customer)
                .Include(d => d.Supplier)
                .Include(d => d.Vehicle)
                .FirstOrDefaultAsync(m => m.DeliveryId == id);
            if (delivery == null)
            {
                return NotFound();
            }

            return View(delivery);
        }

        // GET: Deliveries/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Address");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Address");
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "Brand");
            return View();
        }

        // POST: Deliveries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DeliveryId,DeliveryType,SupplierId,CustomerId,VehicleId,ReferenceNumber,ExpectedDeliveryDate,ActualDeliveryDate,Status,TotalAmount,IsProcessed,CreatedDate,Notes,IsDeleted,DeletedDate")] Delivery delivery)
        {
            if (ModelState.IsValid)
            {
                _context.Add(delivery);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Address", delivery.CustomerId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Address", delivery.SupplierId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "Brand", delivery.VehicleId);
            return View(delivery);
        }

        // GET: Deliveries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var delivery = await _context.Deliveries.FindAsync(id);
            if (delivery == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Address", delivery.CustomerId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Address", delivery.SupplierId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "Brand", delivery.VehicleId);
            return View(delivery);
        }

        // POST: Deliveries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DeliveryId,DeliveryType,SupplierId,CustomerId,VehicleId,ReferenceNumber,ExpectedDeliveryDate,ActualDeliveryDate,Status,TotalAmount,IsProcessed,CreatedDate,Notes,IsDeleted,DeletedDate")] Delivery delivery)
        {
            if (id != delivery.DeliveryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(delivery);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeliveryExists(delivery.DeliveryId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Address", delivery.CustomerId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Address", delivery.SupplierId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles, "VehicleId", "Brand", delivery.VehicleId);
            return View(delivery);
        }

        // GET: Deliveries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var delivery = await _context.Deliveries
                .Include(d => d.Customer)
                .Include(d => d.Supplier)
                .Include(d => d.Vehicle)
                .FirstOrDefaultAsync(m => m.DeliveryId == id);
            if (delivery == null)
            {
                return NotFound();
            }

            return View(delivery);
        }

        // POST: Deliveries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var delivery = await _context.Deliveries.FindAsync(id);
            if (delivery != null)
            {
                _context.Deliveries.Remove(delivery);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeliveryExists(int id)
        {
            return _context.Deliveries.Any(e => e.DeliveryId == id);
        }
    }
}
