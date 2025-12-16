using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem_Web.Controllers
{
    /// <summary>
    /// Controller voor het beheren van Leveringen (Deliveries)
    /// Implementeert volledige CRUD operaties met authorization, soft delete en logging
    /// </summary>
    [Authorize]
    public class DeliveriesController : Controller
    {
        private readonly SuntoryDbContext _context;
        private readonly ILogger<DeliveriesController> _logger;

        public DeliveriesController(SuntoryDbContext context, ILogger<DeliveriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================================
        // GET: Deliveries
        // Toegankelijk voor: Alle ingelogde gebruikers
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Deliveries Index pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");

                // Filter soft deleted deliveries en sorteer op verwachte leveringsdatum (nieuwste eerst)
                var deliveries = await _context.Deliveries
                    .Include(d => d.Customer)
                    .Include(d => d.Supplier)
                    .Include(d => d.Vehicle)
                    .Where(d => !d.IsDeleted)
                    .OrderByDescending(d => d.ExpectedDeliveryDate)
                    .ToListAsync();

                return View(deliveries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van deliveries");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de leveringen.";
                return View(new List<Delivery>());
            }
        }

        // =====================================================================
        // GET: Deliveries/Details/5
        // Toegankelijk voor: Alle ingelogde gebruikers
        // =====================================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details aangeroepen zonder ID");
                return NotFound();
            }

            try
            {
                var delivery = await _context.Deliveries
                    .Include(d => d.Customer)
                    .Include(d => d.Supplier)
                    .Include(d => d.Vehicle)
                    .Where(d => !d.IsDeleted)
                    .FirstOrDefaultAsync(m => m.DeliveryId == id);

                if (delivery == null)
                {
                    _logger.LogWarning("Delivery met ID {DeliveryId} niet gevonden", id);
                    return NotFound();
                }

                _logger.LogInformation("Details van Delivery {ReferenceNumber} (ID: {DeliveryId}) bekeken door {User}",
                    delivery.ReferenceNumber, id, User.Identity?.Name ?? "Anonymous");

                return View(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van delivery details voor ID {DeliveryId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de levering details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // GET: Deliveries/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            _logger.LogInformation("Create pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");
            ViewData["CustomerId"] = new SelectList(_context.Customers.Where(c => !c.IsDeleted), "CustomerId", "CustomerName");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "SupplierId", "SupplierName");
            ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => !v.IsDeleted), "VehicleId", "LicensePlate");
            return View();
        }

        // =====================================================================
        // POST: Deliveries/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("DeliveryId,DeliveryType,SupplierId,CustomerId,VehicleId,ReferenceNumber,ExpectedDeliveryDate,ActualDeliveryDate,Status,TotalAmount,IsProcessed,Notes")] Delivery delivery)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Zet created date
                    delivery.CreatedDate = DateTime.Now;
                    delivery.IsDeleted = false;

                    _context.Add(delivery);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Delivery {ReferenceNumber} (ID: {DeliveryId}) aangemaakt door {User}",
                        delivery.ReferenceNumber, delivery.DeliveryId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Levering '{delivery.ReferenceNumber}' succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij aanmaken van delivery {ReferenceNumber}", delivery.ReferenceNumber);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het aanmaken van de levering.";
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers.Where(c => !c.IsDeleted), "CustomerId", "CustomerName", delivery.CustomerId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "SupplierId", "SupplierName", delivery.SupplierId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => !v.IsDeleted), "VehicleId", "LicensePlate", delivery.VehicleId);
            return View(delivery);
        }

        // =====================================================================
        // GET: Deliveries/Edit/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit aangeroepen zonder ID");
                return NotFound();
            }

            try
            {
                var delivery = await _context.Deliveries
                    .Where(d => !d.IsDeleted)
                    .FirstOrDefaultAsync(d => d.DeliveryId == id);

                if (delivery == null)
                {
                    _logger.LogWarning("Delivery met ID {DeliveryId} niet gevonden voor edit", id);
                    return NotFound();
                }

                _logger.LogInformation("Edit pagina geopend voor Delivery {ReferenceNumber} (ID: {DeliveryId}) door {User}",
                    delivery.ReferenceNumber, id, User.Identity?.Name ?? "Anonymous");

                ViewData["CustomerId"] = new SelectList(_context.Customers.Where(c => !c.IsDeleted), "CustomerId", "CustomerName", delivery.CustomerId);
                ViewData["SupplierId"] = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "SupplierId", "SupplierName", delivery.SupplierId);
                ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => !v.IsDeleted), "VehicleId", "LicensePlate", delivery.VehicleId);
                return View(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van delivery voor edit met ID {DeliveryId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de levering.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Deliveries/Edit/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("DeliveryId,DeliveryType,SupplierId,CustomerId,VehicleId,ReferenceNumber,ExpectedDeliveryDate,ActualDeliveryDate,Status,TotalAmount,IsProcessed,CreatedDate,Notes,IsDeleted,DeletedDate")] Delivery delivery)
        {
            if (id != delivery.DeliveryId)
            {
                _logger.LogWarning("ID mismatch in Edit: URL ID {UrlId} vs Delivery ID {DeliveryId}", id, delivery.DeliveryId);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(delivery);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Delivery {ReferenceNumber} (ID: {DeliveryId}) gewijzigd door {User}",
                        delivery.ReferenceNumber, delivery.DeliveryId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Levering '{delivery.ReferenceNumber}' succesvol gewijzigd!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!DeliveryExists(delivery.DeliveryId))
                    {
                        _logger.LogWarning("Delivery met ID {DeliveryId} niet meer gevonden tijdens update", delivery.DeliveryId);
                        TempData["ErrorMessage"] = "De levering bestaat niet meer in de database.";
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency fout bij wijzigen van delivery {DeliveryId}", delivery.DeliveryId);
                        TempData["ErrorMessage"] = "Er is een conflict opgetreden. Mogelijk is de levering al gewijzigd door een andere gebruiker.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij wijzigen van delivery {DeliveryId}", delivery.DeliveryId);
                    TempData["ErrorMessage"] = "Er is een fout opgetreden bij het wijzigen van de levering.";
                }
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers.Where(c => !c.IsDeleted), "CustomerId", "CustomerName", delivery.CustomerId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "SupplierId", "SupplierName", delivery.SupplierId);
            ViewData["VehicleId"] = new SelectList(_context.Vehicles.Where(v => !v.IsDeleted), "VehicleId", "LicensePlate", delivery.VehicleId);
            return View(delivery);
        }

        // =====================================================================
        // GET: Deliveries/Delete/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete aangeroepen zonder ID");
                return NotFound();
            }

            try
            {
                var delivery = await _context.Deliveries
                    .Include(d => d.Customer)
                    .Include(d => d.Supplier)
                    .Include(d => d.Vehicle)
                    .Where(d => !d.IsDeleted)
                    .FirstOrDefaultAsync(m => m.DeliveryId == id);

                if (delivery == null)
                {
                    _logger.LogWarning("Delivery met ID {DeliveryId} niet gevonden voor delete", id);
                    return NotFound();
                }

                _logger.LogInformation("Delete confirmatie pagina geopend voor Delivery {ReferenceNumber} (ID: {DeliveryId}) door {User}",
                    delivery.ReferenceNumber, id, User.Identity?.Name ?? "Anonymous");

                return View(delivery);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van delivery voor delete met ID {DeliveryId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de levering.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Deliveries/Delete/5
        // SOFT DELETE implementatie
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var delivery = await _context.Deliveries.FindAsync(id);

                if (delivery != null)
                {
                    // SOFT DELETE: markeer als verwijderd in plaats van hard delete
                    delivery.IsDeleted = true;
                    delivery.DeletedDate = DateTime.Now;
                    _context.Deliveries.Update(delivery);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Delivery {ReferenceNumber} (ID: {DeliveryId}) soft deleted door {User}",
                        delivery.ReferenceNumber, id, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Levering '{delivery.ReferenceNumber}' succesvol verwijderd!";
                }
                else
                {
                    _logger.LogWarning("Delivery met ID {DeliveryId} niet gevonden voor delete", id);
                    TempData["ErrorMessage"] = "De levering kon niet worden gevonden.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwijderen van delivery met ID {DeliveryId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwijderen van de levering.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // HELPER METHODE: Check of delivery bestaat
        // =====================================================================
        private bool DeliveryExists(int id)
        {
            return _context.Deliveries.Any(e => e.DeliveryId == id && !e.IsDeleted);
        }
    }
}
