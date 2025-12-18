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
                    await _context.SaveChangesAsync(); // EERST product opslaan zodat ProductId beschikbaar is

                    _logger.LogInformation("Product {ProductName} (ID: {ProductId}) aangemaakt door {User}", 
                        product.ProductName, product.ProductId, User.Identity?.Name ?? "Anonymous");

                    // CHECK EN MAAK STOCK ALERT AAN indien nodig (NU met correcte ProductId)
                    await CheckAndCreateStockAlert(product);

                    TempData["SuccessMessage"] = $"Product '{product.ProductName}' succesvol toegevoegd!";
                    
                    // Waarschuw gebruiker als voorraad laag is
                    if (product.StockQuantity < product.MinimumStock)
                    {
                        TempData["WarningMessage"] = $"LET OP: Voorraad ({product.StockQuantity}) is onder minimum ({product.MinimumStock}). Een stock alert is automatisch aangemaakt.";
                    }
                    
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
                    // Haal oude waarden op voor vergelijking VOORDAT we updaten
                    var originalProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                    int previousStock = originalProduct?.StockQuantity ?? 0;

                    _context.Update(product);
                    await _context.SaveChangesAsync(); // EERST product updaten

                    _logger.LogInformation("Product {ProductName} (ID: {ProductId}) gewijzigd door {User} - Voorraad: {OldStock} -> {NewStock}", 
                        product.ProductName, product.ProductId, User.Identity?.Name ?? "Anonymous", previousStock, product.StockQuantity);

                    // CHECK EN UPDATE STOCK ALERTS (NA het opslaan van product)
                    await CheckAndUpdateStockAlerts(product, previousStock);

                    TempData["SuccessMessage"] = $"Product '{product.ProductName}' succesvol gewijzigd!";
                    
                    // Waarschuw gebruiker als voorraad laag is
                    if (product.StockQuantity < product.MinimumStock)
                    {
                        TempData["WarningMessage"] = $"LET OP: Voorraad ({product.StockQuantity}) is onder minimum ({product.MinimumStock}).";
                    }
                    
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
        // HELPER METHODS: Stock Alert Management
        // =====================================================================
        
        /// <summary>
        /// Controleert voorraad en maakt stock alert aan indien nodig
        /// </summary>
        private async Task CheckAndCreateStockAlert(Product product)
        {
            if (product.StockQuantity >= product.MinimumStock)
            {
                _logger.LogInformation("Product {ProductName} (ID: {ProductId}) heeft voldoende voorraad ({Stock} >= {Min}), geen alert nodig", 
                    product.ProductName, product.ProductId, product.StockQuantity, product.MinimumStock);
                return; // Voorraad is OK, geen alert nodig
            }

            try
            {
                _logger.LogInformation("Checking for existing alerts for Product {ProductId}", product.ProductId);
                
                // Check of er al een actieve alert bestaat voor dit product
                var existingAlert = await _context.StockAlerts
                    .FirstOrDefaultAsync(sa => sa.ProductId == product.ProductId 
                        && sa.Status == "Active" 
                        && !sa.IsDeleted);

                if (existingAlert == null)
                {
                    // Bepaal alert type
                    string alertType = product.StockQuantity == 0 ? "Out of Stock" : 
                                     product.StockQuantity < (product.MinimumStock / 2) ? "Critical" : 
                                     "Low Stock";

                    var alert = new StockAlert
                    {
                        ProductId = product.ProductId,
                        AlertType = alertType,
                        Status = "Active",
                        CreatedDate = DateTime.Now,
                        Notes = $"Voorraad is {product.StockQuantity} stuks, minimum is {product.MinimumStock}"
                    };

                    _context.StockAlerts.Add(alert);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("? Stock Alert SUCCESVOL aangemaakt voor Product {ProductName} (ID: {ProductId}) - Type: {AlertType}, StockAlertId: {StockAlertId}", 
                        product.ProductName, product.ProductId, alertType, alert.StockAlertId);
                }
                else
                {
                    _logger.LogInformation("Stock Alert bestaat al voor Product {ProductName} (ID: {ProductId}) - Alert ID: {AlertId}", 
                        product.ProductName, product.ProductId, existingAlert.StockAlertId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? FOUT bij aanmaken van stock alert voor Product {ProductName} (ID: {ProductId})", 
                    product.ProductName, product.ProductId);
                // We gooien de exception NIET verder, zodat het aanmaken van het product niet faalt
            }
        }

        /// <summary>
        /// Update stock alerts gebaseerd op voorraadwijzigingen
        /// </summary>
        private async Task CheckAndUpdateStockAlerts(Product product, int previousStock)
        {
            try
            {
                _logger.LogInformation("CheckAndUpdateStockAlerts voor Product {ProductId}: Voorraad {OldStock} -> {NewStock}, Minimum: {MinStock}", 
                    product.ProductId, previousStock, product.StockQuantity, product.MinimumStock);

                // Scenario 1: Voorraad is nu ONDER minimum (maak of update alert)
                if (product.StockQuantity < product.MinimumStock)
                {
                    _logger.LogInformation("Voorraad ONDER minimum - checking for existing alert");
                    
                    var existingAlert = await _context.StockAlerts
                        .FirstOrDefaultAsync(sa => sa.ProductId == product.ProductId 
                            && sa.Status == "Active" 
                            && !sa.IsDeleted);

                    string alertType = product.StockQuantity == 0 ? "Out of Stock" : 
                                     product.StockQuantity < (product.MinimumStock / 2) ? "Critical" : 
                                     "Low Stock";

                    if (existingAlert == null)
                    {
                        // Maak nieuwe alert
                        var alert = new StockAlert
                        {
                            ProductId = product.ProductId,
                            AlertType = alertType,
                            Status = "Active",
                            CreatedDate = DateTime.Now,
                            Notes = $"Voorraad is {product.StockQuantity} stuks, minimum is {product.MinimumStock}"
                        };

                        _context.StockAlerts.Add(alert);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("? Stock Alert AANGEMAAKT voor Product {ProductName} (ID: {ProductId}) - Type: {AlertType}, AlertId: {AlertId}", 
                            product.ProductName, product.ProductId, alertType, alert.StockAlertId);
                    }
                    else
                    {
                        // Update bestaande alert
                        existingAlert.AlertType = alertType;
                        existingAlert.Notes = $"Voorraad is {product.StockQuantity} stuks, minimum is {product.MinimumStock}. Laatst gewijzigd: {DateTime.Now:dd-MM-yyyy HH:mm}";
                        _context.StockAlerts.Update(existingAlert);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("?? Stock Alert GEUPDATE voor Product {ProductName} (ID: {ProductId}) - Type: {AlertType}", 
                            product.ProductName, product.ProductId, alertType);
                    }
                }
                // Scenario 2: Voorraad was ONDER minimum, maar is nu BOVEN minimum (resolve alerts)
                else if (previousStock < product.MinimumStock && product.StockQuantity >= product.MinimumStock)
                {
                    _logger.LogInformation("Voorraad HERSTELD - resolving active alerts");
                    
                    var activeAlerts = await _context.StockAlerts
                        .Where(sa => sa.ProductId == product.ProductId 
                            && sa.Status == "Active" 
                            && !sa.IsDeleted)
                        .ToListAsync();

                    foreach (var alert in activeAlerts)
                    {
                        alert.Status = "Resolved";
                        alert.ResolvedDate = DateTime.Now;
                        alert.Notes += $" - Opgelost: voorraad is nu {product.StockQuantity} stuks (boven minimum van {product.MinimumStock})";
                        _context.StockAlerts.Update(alert);
                    }

                    if (activeAlerts.Any())
                    {
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("? {Count} Stock Alert(s) OPGELOST voor Product {ProductName} (ID: {ProductId})", 
                            activeAlerts.Count, product.ProductName, product.ProductId);
                    }
                }
                else
                {
                    _logger.LogInformation("Geen stock alert actie nodig voor Product {ProductId}", product.ProductId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? FOUT bij updaten van stock alerts voor Product {ProductName} (ID: {ProductId})", 
                    product.ProductName, product.ProductId);
                // We gooien de exception NIET verder, zodat het updaten van het product niet faalt
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id && !e.IsDeleted);
        }
    }
}
