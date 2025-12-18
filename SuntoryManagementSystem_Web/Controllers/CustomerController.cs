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
    /// Controller voor het beheren van Klanten (Customers)
    /// Implementeert volledige CRUD operaties met authorization, soft delete en logging
    /// </summary>
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly SuntoryDbContext _context;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(SuntoryDbContext context, ILogger<CustomersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================================
        // GET: Customers
        // Toegankelijk voor: Alle ingelogde gebruikers
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Customers Index pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");

                // Filter soft deleted customers en sorteer op status en naam
                var customers = await _context.Customers
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Status == "Active" ? 0 : 1)  // Actieve eerst
                    .ThenBy(c => c.CustomerName)
                    .ToListAsync();

                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van customers");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de klanten.";
                return View(new List<Customer>());
            }
        }

        // =====================================================================
        // GET: Customers/Details/5
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
                var customer = await _context.Customers
                    .Where(c => !c.IsDeleted)
                    .FirstOrDefaultAsync(m => m.CustomerId == id);

                if (customer == null)
                {
                    _logger.LogWarning("Customer met ID {CustomerId} niet gevonden", id);
                    return NotFound();
                }

                _logger.LogInformation("Details van Customer {CustomerName} (ID: {CustomerId}) bekeken door {User}",
                    customer.CustomerName, id, User.Identity?.Name ?? "Anonymous");

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van customer details voor ID {CustomerId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de klant details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // GET: Customers/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            _logger.LogInformation("Create pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");
            return View();
        }

        // =====================================================================
        // POST: Customers/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("CustomerId,CustomerName,Address,PostalCode,City,PhoneNumber,Email,ContactPerson,CustomerType,Status,Notes")] Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Zet created date
                    customer.CreatedDate = DateTime.Now;
                    customer.IsDeleted = false;

                    _context.Add(customer);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Customer {CustomerName} (ID: {CustomerId}) aangemaakt door {User}",
                        customer.CustomerName, customer.CustomerId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Klant '{customer.CustomerName}' succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij aanmaken van customer {CustomerName}", customer.CustomerName);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het aanmaken van de klant.";
            }

            return View(customer);
        }

        // =====================================================================
        // GET: Customers/Edit/5
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
                var customer = await _context.Customers
                    .Where(c => !c.IsDeleted)
                    .FirstOrDefaultAsync(c => c.CustomerId == id);

                if (customer == null)
                {
                    _logger.LogWarning("Customer met ID {CustomerId} niet gevonden voor edit", id);
                    return NotFound();
                }

                _logger.LogInformation("Edit pagina geopend voor Customer {CustomerName} (ID: {CustomerId}) door {User}",
                    customer.CustomerName, id, User.Identity?.Name ?? "Anonymous");

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van customer voor edit met ID {CustomerId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de klant.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Customers/Edit/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,CustomerName,Address,PostalCode,City,PhoneNumber,Email,ContactPerson,CustomerType,Status,CreatedDate,Notes,IsDeleted,DeletedDate")] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                _logger.LogWarning("ID mismatch in Edit: URL ID {UrlId} vs Customer ID {CustomerId}", id, customer.CustomerId);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Customer {CustomerName} (ID: {CustomerId}) gewijzigd door {User}",
                        customer.CustomerName, customer.CustomerId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Klant '{customer.CustomerName}' succesvol gewijzigd!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        _logger.LogWarning("Customer met ID {CustomerId} niet meer gevonden tijdens update", customer.CustomerId);
                        TempData["ErrorMessage"] = "De klant bestaat niet meer in de database.";
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency fout bij wijzigen van customer {CustomerId}", customer.CustomerId);
                        TempData["ErrorMessage"] = "Er is een conflict opgetreden. Mogelijk is de klant al gewijzigd door een andere gebruiker.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij wijzigen van customer {CustomerId}", customer.CustomerId);
                    TempData["ErrorMessage"] = "Er is een fout opgetreden bij het wijzigen van de klant.";
                }
            }

            return View(customer);
        }

        // =====================================================================
        // GET: Customers/Delete/5
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
                var customer = await _context.Customers
                    .Where(c => !c.IsDeleted)
                    .FirstOrDefaultAsync(m => m.CustomerId == id);

                if (customer == null)
                {
                    _logger.LogWarning("Customer met ID {CustomerId} niet gevonden voor delete", id);
                    return NotFound();
                }

                _logger.LogInformation("Delete confirmatie pagina geopend voor Customer {CustomerName} (ID: {CustomerId}) door {User}",
                    customer.CustomerName, id, User.Identity?.Name ?? "Anonymous");

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van customer voor delete met ID {CustomerId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de klant.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Customers/Delete/5
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
                var customer = await _context.Customers.FindAsync(id);

                if (customer != null)
                {
                    // SOFT DELETE: markeer als verwijderd in plaats van hard delete
                    customer.IsDeleted = true;
                    customer.DeletedDate = DateTime.Now;
                    _context.Customers.Update(customer);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Customer {CustomerName} (ID: {CustomerId}) soft deleted door {User}",
                        customer.CustomerName, id, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Klant '{customer.CustomerName}' succesvol verwijderd!";
                }
                else
                {
                    _logger.LogWarning("Customer met ID {CustomerId} niet gevonden voor delete", id);
                    TempData["ErrorMessage"] = "De klant kon niet worden gevonden.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwijderen van customer met ID {CustomerId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwijderen van de klant.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // HELPER METHODE: Check of customer bestaat
        // =====================================================================
        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id && !e.IsDeleted);
        }
    }
}
