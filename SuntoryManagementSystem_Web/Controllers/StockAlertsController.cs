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
    /// Controller voor het beheren van Voorraad Alerts (StockAlerts)
    /// Implementeert volledige CRUD operaties met authorization, soft delete en logging
    /// </summary>
    [Authorize]
    public class StockAlertsController : Controller
    {
        private readonly SuntoryDbContext _context;
        private readonly ILogger<StockAlertsController> _logger;

        public StockAlertsController(SuntoryDbContext context, ILogger<StockAlertsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================================
        // GET: StockAlerts
        // Toegankelijk voor: Alle ingelogde gebruikers
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("StockAlerts Index pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");

                // Filter soft deleted stock alerts en sorteer op alert datum (nieuwste eerst)
                var stockAlerts = await _context.StockAlerts
                    .Include(s => s.Product)
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.CreatedDate)
                    .ToListAsync();

                return View(stockAlerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van stock alerts");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voorraad alerts.";
                return View(new List<StockAlert>());
            }
        }

        // =====================================================================
        // GET: StockAlerts/Details/5
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
                var stockAlert = await _context.StockAlerts
                    .Include(s => s.Product)
                    .Where(s => !s.IsDeleted)
                    .FirstOrDefaultAsync(m => m.StockAlertId == id);

                if (stockAlert == null)
                {
                    _logger.LogWarning("StockAlert met ID {StockAlertId} niet gevonden", id);
                    return NotFound();
                }

                _logger.LogInformation("Details van StockAlert ID {StockAlertId} bekeken door {User}", 
                    id, User.Identity?.Name ?? "Anonymous");

                return View(stockAlert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van stock alert details voor ID {StockAlertId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voorraad alert details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // GET: StockAlerts/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            _logger.LogInformation("Create pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");
            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName");
            return View();
        }

        // =====================================================================
        // POST: StockAlerts/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("StockAlertId,ProductId,AlertDate,AlertType,CurrentStock,ThresholdLevel,IsResolved,Notes")] StockAlert stockAlert)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Zet created date
                    stockAlert.CreatedDate = DateTime.Now;
                    stockAlert.IsDeleted = false;

                    _context.Add(stockAlert);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("StockAlert ID {StockAlertId} aangemaakt door {User} - Product: {ProductId}, Type: {Type}, Status: {Status}", 
                        stockAlert.StockAlertId, User.Identity?.Name ?? "Anonymous", 
                        stockAlert.ProductId, stockAlert.AlertType, stockAlert.Status);

                    TempData["SuccessMessage"] = $"Voorraad alert succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij aanmaken van stock alert voor Product ID {ProductId}", stockAlert.ProductId);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het aanmaken van de voorraad alert.";
            }

            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName", stockAlert.ProductId);
            return View(stockAlert);
        }

        // =====================================================================
        // GET: StockAlerts/Edit/5
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
                var stockAlert = await _context.StockAlerts
                    .Where(s => !s.IsDeleted)
                    .FirstOrDefaultAsync(s => s.StockAlertId == id);

                if (stockAlert == null)
                {
                    _logger.LogWarning("StockAlert met ID {StockAlertId} niet gevonden voor edit", id);
                    return NotFound();
                }

                _logger.LogInformation("Edit pagina geopend voor StockAlert ID {StockAlertId} door {User}", 
                    id, User.Identity?.Name ?? "Anonymous");

                ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName", stockAlert.ProductId);
                return View(stockAlert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van stock alert voor edit met ID {StockAlertId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voorraad alert.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: StockAlerts/Edit/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("StockAlertId,ProductId,AlertDate,AlertType,CurrentStock,ThresholdLevel,IsResolved,CreatedDate,Notes,IsDeleted,DeletedDate")] StockAlert stockAlert)
        {
            if (id != stockAlert.StockAlertId)
            {
                _logger.LogWarning("ID mismatch in Edit: URL ID {UrlId} vs StockAlert ID {StockAlertId}", id, stockAlert.StockAlertId);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockAlert);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("StockAlert ID {StockAlertId} gewijzigd door {User}", 
                        stockAlert.StockAlertId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Voorraad alert succesvol gewijzigd!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!StockAlertExists(stockAlert.StockAlertId))
                    {
                        _logger.LogWarning("StockAlert met ID {StockAlertId} niet meer gevonden tijdens update", stockAlert.StockAlertId);
                        TempData["ErrorMessage"] = "De voorraad alert bestaat niet meer in de database.";
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency fout bij wijzigen van stock alert {StockAlertId}", stockAlert.StockAlertId);
                        TempData["ErrorMessage"] = "Er is een conflict opgetreden. Mogelijk is de voorraad alert al gewijzigd door een andere gebruiker.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij wijzigen van stock alert {StockAlertId}", stockAlert.StockAlertId);
                    TempData["ErrorMessage"] = "Er is een fout opgetreden bij het wijzigen van de voorraad alert.";
                }
            }

            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName", stockAlert.ProductId);
            return View(stockAlert);
        }

        // =====================================================================
        // GET: StockAlerts/Delete/5
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
                var stockAlert = await _context.StockAlerts
                    .Include(s => s.Product)
                    .Where(s => !s.IsDeleted)
                    .FirstOrDefaultAsync(m => m.StockAlertId == id);

                if (stockAlert == null)
                {
                    _logger.LogWarning("StockAlert met ID {StockAlertId} niet gevonden voor delete", id);
                    return NotFound();
                }

                _logger.LogInformation("Delete confirmatie pagina geopend voor StockAlert ID {StockAlertId} door {User}", 
                    id, User.Identity?.Name ?? "Anonymous");

                return View(stockAlert);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van stock alert voor delete met ID {StockAlertId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voorraad alert.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: StockAlerts/Delete/5
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
                var stockAlert = await _context.StockAlerts.FindAsync(id);

                if (stockAlert != null)
                {
                    // SOFT DELETE: markeer als verwijderd in plaats van hard delete
                    stockAlert.IsDeleted = true;
                    stockAlert.DeletedDate = DateTime.Now;
                    _context.StockAlerts.Update(stockAlert);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("StockAlert ID {StockAlertId} soft deleted door {User}", 
                        id, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Voorraad alert succesvol verwijderd!";
                }
                else
                {
                    _logger.LogWarning("StockAlert met ID {StockAlertId} niet gevonden voor delete", id);
                    TempData["ErrorMessage"] = "De voorraad alert kon niet worden gevonden.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwijderen van stock alert met ID {StockAlertId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwijderen van de voorraad alert.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // HELPER METHODE: Check of stock alert bestaat
        // =====================================================================
        private bool StockAlertExists(int id)
        {
            return _context.StockAlerts.Any(e => e.StockAlertId == id && !e.IsDeleted);
        }
    }
}
