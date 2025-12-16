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
    /// Controller voor het beheren van Levering Items (DeliveryItems)
    /// Implementeert volledige CRUD operaties met authorization, soft delete en logging
    /// </summary>
    [Authorize]
    public class DeliveryItemsController : Controller
    {
        private readonly SuntoryDbContext _context;
        private readonly ILogger<DeliveryItemsController> _logger;

        public DeliveryItemsController(SuntoryDbContext context, ILogger<DeliveryItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================================
        // GET: DeliveryItems
        // Toegankelijk voor: Alle ingelogde gebruikers
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("DeliveryItems Index pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");

                // Filter soft deleted delivery items en sorteer op delivery date
                var deliveryItems = await _context.DeliveryItems
                    .Include(d => d.Delivery)
                    .Include(d => d.Product)
                    .Where(d => !d.IsDeleted)
                    .OrderByDescending(d => d.Delivery.ExpectedDeliveryDate)
                    .ToListAsync();

                return View(deliveryItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van delivery items");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de levering items.";
                return View(new List<DeliveryItem>());
            }
        }

        // =====================================================================
        // GET: DeliveryItems/Details/5
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
                var deliveryItem = await _context.DeliveryItems
                    .Include(d => d.Delivery)
                    .Include(d => d.Product)
                    .Where(d => !d.IsDeleted)
                    .FirstOrDefaultAsync(m => m.DeliveryItemId == id);

                if (deliveryItem == null)
                {
                    _logger.LogWarning("DeliveryItem met ID {DeliveryItemId} niet gevonden", id);
                    return NotFound();
                }

                _logger.LogInformation("Details van DeliveryItem ID {DeliveryItemId} bekeken door {User}", 
                    id, User.Identity?.Name ?? "Anonymous");

                return View(deliveryItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van delivery item details voor ID {DeliveryItemId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de levering item details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // GET: DeliveryItems/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            _logger.LogInformation("Create pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");
            ViewData["DeliveryId"] = new SelectList(_context.Deliveries.Where(d => !d.IsDeleted), "DeliveryId", "DeliveryId");
            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName");
            return View();
        }

        // =====================================================================
        // POST: DeliveryItems/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("DeliveryItemId,DeliveryId,ProductId,Quantity,UnitPrice,Subtotal")] DeliveryItem deliveryItem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    deliveryItem.IsDeleted = false;

                    _context.Add(deliveryItem);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("DeliveryItem ID {DeliveryItemId} aangemaakt door {User} - Delivery: {DeliveryId}, Product: {ProductId}, Quantity: {Quantity}", 
                        deliveryItem.DeliveryItemId, User.Identity?.Name ?? "Anonymous", 
                        deliveryItem.DeliveryId, deliveryItem.ProductId, deliveryItem.Quantity);

                    TempData["SuccessMessage"] = $"Levering item succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij aanmaken van delivery item voor Delivery ID {DeliveryId}", deliveryItem.DeliveryId);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het aanmaken van het levering item.";
            }

            ViewData["DeliveryId"] = new SelectList(_context.Deliveries.Where(d => !d.IsDeleted), "DeliveryId", "DeliveryId", deliveryItem.DeliveryId);
            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName", deliveryItem.ProductId);
            return View(deliveryItem);
        }

        // =====================================================================
        // GET: DeliveryItems/Edit/5
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
                var deliveryItem = await _context.DeliveryItems
                    .Where(d => !d.IsDeleted)
                    .FirstOrDefaultAsync(d => d.DeliveryItemId == id);

                if (deliveryItem == null)
                {
                    _logger.LogWarning("DeliveryItem met ID {DeliveryItemId} niet gevonden voor edit", id);
                    return NotFound();
                }

                _logger.LogInformation("Edit pagina geopend voor DeliveryItem ID {DeliveryItemId} door {User}", 
                    id, User.Identity?.Name ?? "Anonymous");

                ViewData["DeliveryId"] = new SelectList(_context.Deliveries.Where(d => !d.IsDeleted), "DeliveryId", "DeliveryId", deliveryItem.DeliveryId);
                ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName", deliveryItem.ProductId);
                return View(deliveryItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van delivery item voor edit met ID {DeliveryItemId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van het levering item.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: DeliveryItems/Edit/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("DeliveryItemId,DeliveryId,ProductId,Quantity,UnitPrice,Subtotal,CreatedDate,IsDeleted,DeletedDate")] DeliveryItem deliveryItem)
        {
            if (id != deliveryItem.DeliveryItemId)
            {
                _logger.LogWarning("ID mismatch in Edit: URL ID {UrlId} vs DeliveryItem ID {DeliveryItemId}", id, deliveryItem.DeliveryItemId);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deliveryItem);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("DeliveryItem ID {DeliveryItemId} gewijzigd door {User}", 
                        deliveryItem.DeliveryItemId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Levering item succesvol gewijzigd!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!DeliveryItemExists(deliveryItem.DeliveryItemId))
                    {
                        _logger.LogWarning("DeliveryItem met ID {DeliveryItemId} niet meer gevonden tijdens update", deliveryItem.DeliveryItemId);
                        TempData["ErrorMessage"] = "Het levering item bestaat niet meer in de database.";
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency fout bij wijzigen van delivery item {DeliveryItemId}", deliveryItem.DeliveryItemId);
                        TempData["ErrorMessage"] = "Er is een conflict opgetreden. Mogelijk is het levering item al gewijzigd door een andere gebruiker.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij wijzigen van delivery item {DeliveryItemId}", deliveryItem.DeliveryItemId);
                    TempData["ErrorMessage"] = "Er is een fout opgetreden bij het wijzigen van het levering item.";
                }
            }

            ViewData["DeliveryId"] = new SelectList(_context.Deliveries.Where(d => !d.IsDeleted), "DeliveryId", "DeliveryId", deliveryItem.DeliveryId);
            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName", deliveryItem.ProductId);
            return View(deliveryItem);
        }

        // =====================================================================
        // GET: DeliveryItems/Delete/5
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
                var deliveryItem = await _context.DeliveryItems
                    .Include(d => d.Delivery)
                    .Include(d => d.Product)
                    .Where(d => !d.IsDeleted)
                    .FirstOrDefaultAsync(m => m.DeliveryItemId == id);

                if (deliveryItem == null)
                {
                    _logger.LogWarning("DeliveryItem met ID {DeliveryItemId} niet gevonden voor delete", id);
                    return NotFound();
                }

                _logger.LogInformation("Delete confirmatie pagina geopend voor DeliveryItem ID {DeliveryItemId} door {User}", 
                    id, User.Identity?.Name ?? "Anonymous");

                return View(deliveryItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van delivery item voor delete met ID {DeliveryItemId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van het levering item.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: DeliveryItems/Delete/5
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
                var deliveryItem = await _context.DeliveryItems.FindAsync(id);

                if (deliveryItem != null)
                {
                    // SOFT DELETE: markeer als verwijderd in plaats van hard delete
                    deliveryItem.IsDeleted = true;
                    deliveryItem.DeletedDate = DateTime.Now;
                    _context.DeliveryItems.Update(deliveryItem);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("DeliveryItem ID {DeliveryItemId} soft deleted door {User}", 
                        id, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Levering item succesvol verwijderd!";
                }
                else
                {
                    _logger.LogWarning("DeliveryItem met ID {DeliveryItemId} niet gevonden voor delete", id);
                    TempData["ErrorMessage"] = "Het levering item kon niet worden gevonden.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwijderen van delivery item met ID {DeliveryItemId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwijderen van het levering item.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // HELPER METHODE: Check of delivery item bestaat
        // =====================================================================
        private bool DeliveryItemExists(int id)
        {
            return _context.DeliveryItems.Any(e => e.DeliveryItemId == id && !e.IsDeleted);
        }
    }
}
