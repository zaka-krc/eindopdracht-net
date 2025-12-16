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
    /// Controller voor het beheren van Voorraad Aanpassingen (StockAdjustments)
    /// Implementeert volledige CRUD operaties met authorization, soft delete en logging
    /// </summary>
    [Authorize]
    public class StockAdjustmentsController : Controller
    {
        private readonly SuntoryDbContext _context;
        private readonly ILogger<StockAdjustmentsController> _logger;

        public StockAdjustmentsController(SuntoryDbContext context, ILogger<StockAdjustmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================================
        // GET: StockAdjustments
        // Toegankelijk voor: Alle ingelogde gebruikers
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("StockAdjustments Index pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");

                // Filter soft deleted stock adjustments en sorteer op aanpassingsdatum (nieuwste eerst)
                var stockAdjustments = await _context.StockAdjustments
                    .Include(s => s.Product)
                    .Where(s => !s.IsDeleted)
                    .OrderByDescending(s => s.AdjustmentDate)
                    .ToListAsync();

                return View(stockAdjustments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van stock adjustments");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voorraad aanpassingen.";
                return View(new List<StockAdjustment>());
            }
        }

        // =====================================================================
        // GET: StockAdjustments/Details/5
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
                var stockAdjustment = await _context.StockAdjustments
                    .Include(s => s.Product)
                    .Where(s => !s.IsDeleted)
                    .FirstOrDefaultAsync(m => m.StockAdjustmentId == id);

                if (stockAdjustment == null)
                {
                    _logger.LogWarning("StockAdjustment met ID {StockAdjustmentId} niet gevonden", id);
                    return NotFound();
                }

                _logger.LogInformation("Details van StockAdjustment ID {StockAdjustmentId} bekeken door {User}", 
                    id, User.Identity?.Name ?? "Anonymous");

                return View(stockAdjustment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van stock adjustment details voor ID {StockAdjustmentId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voorraad aanpassing details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // GET: StockAdjustments/Create
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
        // POST: StockAdjustments/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("StockAdjustmentId,ProductId,AdjustmentDate,AdjustmentType,Quantity,Reason,Notes")] StockAdjustment stockAdjustment)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    stockAdjustment.AdjustmentDate = DateTime.Now;
                    stockAdjustment.IsDeleted = false;

                    _context.Add(stockAdjustment);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("StockAdjustment ID {StockAdjustmentId} aangemaakt door {User} - Product: {ProductId}, Type: {Type}, Change: {Change}", 
                        stockAdjustment.StockAdjustmentId, User.Identity?.Name ?? "Anonymous", 
                        stockAdjustment.ProductId, stockAdjustment.AdjustmentType, stockAdjustment.QuantityChange);

                    TempData["SuccessMessage"] = $"Voorraad aanpassing succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij aanmaken van stock adjustment voor Product ID {ProductId}", stockAdjustment.ProductId);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het aanmaken van de voorraad aanpassing.";
            }

            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName", stockAdjustment.ProductId);
            return View(stockAdjustment);
        }

        // =====================================================================
        // GET: StockAdjustments/Edit/5
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
                var stockAdjustment = await _context.StockAdjustments
                    .Where(s => !s.IsDeleted)
                    .FirstOrDefaultAsync(s => s.StockAdjustmentId == id);

                if (stockAdjustment == null)
                {
                    _logger.LogWarning("StockAdjustment met ID {StockAdjustmentId} niet gevonden voor edit", id);
                    return NotFound();
                }

                _logger.LogInformation("Edit pagina geopend voor StockAdjustment ID {StockAdjustmentId} door {User}", 
                    id, User.Identity?.Name ?? "Anonymous");

                ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName", stockAdjustment.ProductId);
                return View(stockAdjustment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van stock adjustment voor edit met ID {StockAdjustmentId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voorraad aanpassing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: StockAdjustments/Edit/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("StockAdjustmentId,ProductId,AdjustmentDate,AdjustmentType,Quantity,Reason,CreatedDate,Notes,IsDeleted,DeletedDate")] StockAdjustment stockAdjustment)
        {
            if (id != stockAdjustment.StockAdjustmentId)
            {
                _logger.LogWarning("ID mismatch in Edit: URL ID {UrlId} vs StockAdjustment ID {StockAdjustmentId}", id, stockAdjustment.StockAdjustmentId);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockAdjustment);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("StockAdjustment ID {StockAdjustmentId} gewijzigd door {User}", 
                        stockAdjustment.StockAdjustmentId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Voorraad aanpassing succesvol gewijzigd!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!StockAdjustmentExists(stockAdjustment.StockAdjustmentId))
                    {
                        _logger.LogWarning("StockAdjustment met ID {StockAdjustmentId} niet meer gevonden tijdens update", stockAdjustment.StockAdjustmentId);
                        TempData["ErrorMessage"] = "De voorraad aanpassing bestaat niet meer in de database.";
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency fout bij wijzigen van stock adjustment {StockAdjustmentId}", stockAdjustment.StockAdjustmentId);
                        TempData["ErrorMessage"] = "Er is een conflict opgetreden. Mogelijk is de voorraad aanpassing al gewijzigd door een andere gebruiker.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij wijzigen van stock adjustment {StockAdjustmentId}", stockAdjustment.StockAdjustmentId);
                    TempData["ErrorMessage"] = "Er is een fout opgetreden bij het wijzigen van de voorraad aanpassing.";
                }
            }

            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => !p.IsDeleted), "ProductId", "ProductName", stockAdjustment.ProductId);
            return View(stockAdjustment);
        }

        // =====================================================================
        // GET: StockAdjustments/Delete/5
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
                var stockAdjustment = await _context.StockAdjustments
                    .Include(s => s.Product)
                    .Where(s => !s.IsDeleted)
                    .FirstOrDefaultAsync(m => m.StockAdjustmentId == id);

                if (stockAdjustment == null)
                {
                    _logger.LogWarning("StockAdjustment met ID {StockAdjustmentId} niet gevonden voor delete", id);
                    return NotFound();
                }

                _logger.LogInformation("Delete confirmatie pagina geopend voor StockAdjustment ID {StockAdjustmentId} door {User}", 
                    id, User.Identity?.Name ?? "Anonymous");

                return View(stockAdjustment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van stock adjustment voor delete met ID {StockAdjustmentId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voorraad aanpassing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: StockAdjustments/Delete/5
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
                var stockAdjustment = await _context.StockAdjustments.FindAsync(id);

                if (stockAdjustment != null)
                {
                    // SOFT DELETE: markeer als verwijderd in plaats van hard delete
                    stockAdjustment.IsDeleted = true;
                    stockAdjustment.DeletedDate = DateTime.Now;
                    _context.StockAdjustments.Update(stockAdjustment);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("StockAdjustment ID {StockAdjustmentId} soft deleted door {User}", 
                        id, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Voorraad aanpassing succesvol verwijderd!";
                }
                else
                {
                    _logger.LogWarning("StockAdjustment met ID {StockAdjustmentId} niet gevonden voor delete", id);
                    TempData["ErrorMessage"] = "De voorraad aanpassing kon niet worden gevonden.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwijderen van stock adjustment met ID {StockAdjustmentId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwijderen van de voorraad aanpassing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // HELPER METHODE: Check of stock adjustment bestaat
        // =====================================================================
        private bool StockAdjustmentExists(int id)
        {
            return _context.StockAdjustments.Any(e => e.StockAdjustmentId == id && !e.IsDeleted);
        }
    }
}
