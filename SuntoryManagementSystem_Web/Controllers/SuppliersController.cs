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

namespace SuntoryManagementSystem_Web
{
    /// <summary>
    /// Controller voor het beheren van Leveranciers (Suppliers)
    /// Implementeert volledige CRUD operaties met authorization, soft delete en logging
    /// </summary>
    [Authorize]
    public class SuppliersController : Controller
    {
        private readonly SuntoryDbContext _context;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(SuntoryDbContext context, ILogger<SuppliersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================================
        // GET: Suppliers
        // Toegankelijk voor: Alle ingelogde gebruikers
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Suppliers Index pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");

                // Filter soft deleted suppliers en sorteer op status en naam
                var suppliers = await _context.Suppliers
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Status == "Active" ? 0 : 1)  // Actieve eerst
                    .ThenBy(s => s.SupplierName)
                    .ToListAsync();

                return View(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van suppliers");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de leveranciers.";
                return View(new List<Supplier>());
            }
        }

        // =====================================================================
        // GET: Suppliers/Details/5
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
                var supplier = await _context.Suppliers
                    .Where(s => !s.IsDeleted)
                    .FirstOrDefaultAsync(m => m.SupplierId == id);

                if (supplier == null)
                {
                    _logger.LogWarning("Supplier met ID {SupplierId} niet gevonden", id);
                    return NotFound();
                }

                _logger.LogInformation("Details van Supplier {SupplierName} (ID: {SupplierId}) bekeken door {User}",
                    supplier.SupplierName, id, User.Identity?.Name ?? "Anonymous");

                return View(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van supplier details voor ID {SupplierId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de leverancier details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // GET: Suppliers/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            _logger.LogInformation("Create pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");
            return View();
        }

        // =====================================================================
        // POST: Suppliers/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("SupplierId,SupplierName,Address,PostalCode,City,PhoneNumber,Email,ContactPerson,Status,Notes")] Supplier supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Zet created date
                    supplier.CreatedDate = DateTime.Now;
                    supplier.IsDeleted = false;

                    _context.Add(supplier);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Supplier {SupplierName} (ID: {SupplierId}) aangemaakt door {User}",
                        supplier.SupplierName, supplier.SupplierId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Leverancier '{supplier.SupplierName}' succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij aanmaken van supplier {SupplierName}", supplier.SupplierName);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het aanmaken van de leverancier.";
            }

            return View(supplier);
        }

        // =====================================================================
        // GET: Suppliers/Edit/5
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
                var supplier = await _context.Suppliers
                    .Where(s => !s.IsDeleted)
                    .FirstOrDefaultAsync(s => s.SupplierId == id);

                if (supplier == null)
                {
                    _logger.LogWarning("Supplier met ID {SupplierId} niet gevonden voor edit", id);
                    return NotFound();
                }

                _logger.LogInformation("Edit pagina geopend voor Supplier {SupplierName} (ID: {SupplierId}) door {User}",
                    supplier.SupplierName, id, User.Identity?.Name ?? "Anonymous");

                return View(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van supplier voor edit met ID {SupplierId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de leverancier.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Suppliers/Edit/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("SupplierId,SupplierName,Address,PostalCode,City,PhoneNumber,Email,ContactPerson,Status,CreatedDate,Notes,IsDeleted,DeletedDate")] Supplier supplier)
        {
            if (id != supplier.SupplierId)
            {
                _logger.LogWarning("ID mismatch in Edit: URL ID {UrlId} vs Supplier ID {SupplierId}", id, supplier.SupplierId);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(supplier);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Supplier {SupplierName} (ID: {SupplierId}) gewijzigd door {User}",
                        supplier.SupplierName, supplier.SupplierId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Leverancier '{supplier.SupplierName}' succesvol gewijzigd!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!SupplierExists(supplier.SupplierId))
                    {
                        _logger.LogWarning("Supplier met ID {SupplierId} niet meer gevonden tijdens update", supplier.SupplierId);
                        TempData["ErrorMessage"] = "De leverancier bestaat niet meer in de database.";
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency fout bij wijzigen van supplier {SupplierId}", supplier.SupplierId);
                        TempData["ErrorMessage"] = "Er is een conflict opgetreden. Mogelijk is de leverancier al gewijzigd door een andere gebruiker.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij wijzigen van supplier {SupplierId}", supplier.SupplierId);
                    TempData["ErrorMessage"] = "Er is een fout opgetreden bij het wijzigen van de leverancier.";
                }
            }

            return View(supplier);
        }

        // =====================================================================
        // GET: Suppliers/Delete/5
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
                var supplier = await _context.Suppliers
                    .Where(s => !s.IsDeleted)
                    .FirstOrDefaultAsync(m => m.SupplierId == id);

                if (supplier == null)
                {
                    _logger.LogWarning("Supplier met ID {SupplierId} niet gevonden voor delete", id);
                    return NotFound();
                }

                _logger.LogInformation("Delete confirmatie pagina geopend voor Supplier {SupplierName} (ID: {SupplierId}) door {User}",
                    supplier.SupplierName, id, User.Identity?.Name ?? "Anonymous");

                return View(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van supplier voor delete met ID {SupplierId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de leverancier.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Suppliers/Delete/5
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
                var supplier = await _context.Suppliers.FindAsync(id);

                if (supplier != null)
                {
                    // SOFT DELETE: markeer als verwijderd in plaats van hard delete
                    supplier.IsDeleted = true;
                    supplier.DeletedDate = DateTime.Now;
                    _context.Suppliers.Update(supplier);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Supplier {SupplierName} (ID: {SupplierId}) soft deleted door {User}",
                        supplier.SupplierName, id, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Leverancier '{supplier.SupplierName}' succesvol verwijderd!";
                }
                else
                {
                    _logger.LogWarning("Supplier met ID {SupplierId} niet gevonden voor delete", id);
                    TempData["ErrorMessage"] = "De leverancier kon niet worden gevonden.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwijderen van supplier met ID {SupplierId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwijderen van de leverancier.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // HELPER METHODE: Check of supplier bestaat
        // =====================================================================
        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.SupplierId == id && !e.IsDeleted);
        }
    }
}