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
    /// Controller voor het beheren van Producten (Products)
    /// Implementeert volledige CRUD operaties met authorization, soft delete en logging
    /// </summary>
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly SuntoryDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(SuntoryDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================================
        // GET: Products
        // Toegankelijk voor: Alle ingelogde gebruikers
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Products Index pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");

                // Filter soft deleted products en sorteer op productnaam
                var products = await _context.Products
                    .Include(p => p.Supplier)
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.ProductName)
                    .ToListAsync();

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van products");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de producten.";
                return View(new List<Product>());
            }
        }

        // =====================================================================
        // GET: Products/Details/5
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
                var product = await _context.Products
                    .Include(p => p.Supplier)
                    .Where(p => !p.IsDeleted)
                    .FirstOrDefaultAsync(m => m.ProductId == id);

                if (product == null)
                {
                    _logger.LogWarning("Product met ID {ProductId} niet gevonden", id);
                    return NotFound();
                }

                _logger.LogInformation("Details van Product {ProductName} (ID: {ProductId}) bekeken door {User}", 
                    product.ProductName, id, User.Identity?.Name ?? "Anonymous");

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van product details voor ID {ProductId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de product details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // GET: Products/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            _logger.LogInformation("Create pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "SupplierId", "SupplierName");
            return View();
        }

        // =====================================================================
        // POST: Products/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("ProductId,SupplierId,ProductName,Description,SKU,Category,PurchasePrice,SellingPrice,StockQuantity,MinimumStock,IsActive")] Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Zet created date
                    product.CreatedDate = DateTime.Now;
                    product.IsDeleted = false;

                    _context.Add(product);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Product {ProductName} (ID: {ProductId}) aangemaakt door {User}", 
                        product.ProductName, product.ProductId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Product '{product.ProductName}' succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij aanmaken van product {ProductName}", product.ProductName);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het aanmaken van het product.";
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "SupplierId", "SupplierName", product.SupplierId);
            return View(product);
        }

        // =====================================================================
        // GET: Products/Edit/5
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
                var product = await _context.Products
                    .Where(p => !p.IsDeleted)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    _logger.LogWarning("Product met ID {ProductId} niet gevonden voor edit", id);
                    return NotFound();
                }

                _logger.LogInformation("Edit pagina geopend voor Product {ProductName} (ID: {ProductId}) door {User}", 
                    product.ProductName, id, User.Identity?.Name ?? "Anonymous");

                ViewData["SupplierId"] = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "SupplierId", "SupplierName", product.SupplierId);
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van product voor edit met ID {ProductId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van het product.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Products/Edit/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,SupplierId,ProductName,Description,SKU,Category,PurchasePrice,SellingPrice,StockQuantity,MinimumStock,IsActive,CreatedDate,IsDeleted,DeletedDate")] Product product)
        {
            if (id != product.ProductId)
            {
                _logger.LogWarning("ID mismatch in Edit: URL ID {UrlId} vs Product ID {ProductId}", id, product.ProductId);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Product {ProductName} (ID: {ProductId}) gewijzigd door {User}", 
                        product.ProductName, product.ProductId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Product '{product.ProductName}' succesvol gewijzigd!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        _logger.LogWarning("Product met ID {ProductId} niet meer gevonden tijdens update", product.ProductId);
                        TempData["ErrorMessage"] = "Het product bestaat niet meer in de database.";
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency fout bij wijzigen van product {ProductId}", product.ProductId);
                        TempData["ErrorMessage"] = "Er is een conflict opgetreden. Mogelijk is het product al gewijzigd door een andere gebruiker.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij wijzigen van product {ProductId}", product.ProductId);
                    TempData["ErrorMessage"] = "Er is een fout opgetreden bij het wijzigen van het product.";
                }
            }

            ViewData["SupplierId"] = new SelectList(_context.Suppliers.Where(s => !s.IsDeleted), "SupplierId", "SupplierName", product.SupplierId);
            return View(product);
        }

        // =====================================================================
        // GET: Products/Delete/5
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
                var product = await _context.Products
                    .Include(p => p.Supplier)
                    .Where(p => !p.IsDeleted)
                    .FirstOrDefaultAsync(m => m.ProductId == id);

                if (product == null)
                {
                    _logger.LogWarning("Product met ID {ProductId} niet gevonden voor delete", id);
                    return NotFound();
                }

                _logger.LogInformation("Delete confirmatie pagina geopend voor Product {ProductName} (ID: {ProductId}) door {User}", 
                    product.ProductName, id, User.Identity?.Name ?? "Anonymous");

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van product voor delete met ID {ProductId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van het product.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Products/Delete/5
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
                var product = await _context.Products.FindAsync(id);

                if (product != null)
                {
                    // SOFT DELETE: markeer als verwijderd in plaats van hard delete
                    product.IsDeleted = true;
                    product.DeletedDate = DateTime.Now;
                    _context.Products.Update(product);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Product {ProductName} (ID: {ProductId}) soft deleted door {User}", 
                        product.ProductName, id, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Product '{product.ProductName}' succesvol verwijderd!";
                }
                else
                {
                    _logger.LogWarning("Product met ID {ProductId} niet gevonden voor delete", id);
                    TempData["ErrorMessage"] = "Het product kon niet worden gevonden.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwijderen van product met ID {ProductId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwijderen van het product.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // HELPER METHODE: Check of product bestaat
        // =====================================================================
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id && !e.IsDeleted);
        }
    }
}
