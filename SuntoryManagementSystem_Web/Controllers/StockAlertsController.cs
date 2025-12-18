using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem_Web.Controllers
{
    /// <summary>
    /// Controller voor het bekijken van Voorraad Alerts (StockAlerts)
    /// Stock Alerts worden automatisch aangemaakt en beheerd door het systeem
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
        // Toont overzicht van alle stock alerts (actief en opgelost)
        // =====================================================================
        public async Task<IActionResult> Index(string filter = "active")
        {
            try
            {
                _logger.LogInformation("StockAlerts Index pagina bezocht door {User} met filter: {Filter}", 
                    User.Identity?.Name ?? "Anonymous", filter);

                var query = _context.StockAlerts
                    .Include(s => s.Product)
                        .ThenInclude(p => p.Supplier)
                    .Where(s => !s.IsDeleted);

                // Filter op status
                if (filter == "active")
                {
                    query = query.Where(s => s.Status == "Active");
                }
                else if (filter == "resolved")
                {
                    query = query.Where(s => s.Status == "Resolved");
                }
                // "all" toont alles

                var stockAlerts = await query
                    .OrderByDescending(s => s.Status == "Active" ? 1 : 0) // Actieve alerts eerst
                    .ThenByDescending(s => s.CreatedDate)
                    .ToListAsync();

                ViewBag.CurrentFilter = filter;
                ViewBag.ActiveCount = await _context.StockAlerts
                    .CountAsync(s => !s.IsDeleted && s.Status == "Active");
                ViewBag.ResolvedCount = await _context.StockAlerts
                    .CountAsync(s => !s.IsDeleted && s.Status == "Resolved");

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
        // Toont details van een specifieke stock alert
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
                        .ThenInclude(p => p.Supplier)
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
    }
}
