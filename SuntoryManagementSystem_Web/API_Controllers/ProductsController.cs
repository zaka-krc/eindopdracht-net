using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem_Web.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SuntoryDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(SuntoryDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            // Filter soft deleted products
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return BadRequest();
            }

            try
            {
                // Haal oude waarden op voor vergelijking VOORDAT we updaten
                var originalProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                if (originalProduct == null)
                {
                    return NotFound();
                }
                
                int previousStock = originalProduct.StockQuantity;

                // Detach navigation properties to prevent EF from trying to update related entities
                product.Supplier = null;

                _context.Entry(product).State = EntityState.Modified;

                await _context.SaveChangesAsync(); // EERST product updaten

                _logger.LogInformation("API: Product {ProductName} (ID: {ProductId}) gewijzigd - Voorraad: {OldStock} -> {NewStock}", 
                    product.ProductName, product.ProductId, previousStock, product.StockQuantity);

                // CHECK EN UPDATE STOCK ALERTS (NA het opslaan van product)
                await CheckAndUpdateStockAlertsAsync(product, previousStock);

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Fout bij wijzigen van product {ProductId}", product.ProductId);
                return StatusCode(500, new { message = "Er is een fout opgetreden bij het wijzigen van het product." });
            }
        }

        // POST: api/Products
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            try
            {
                // Reset identity column for new entities (EF will generate the ID)
                product.ProductId = 0;
                
                // Ensure required fields have default values if missing
                if (string.IsNullOrWhiteSpace(product.ProductName))
                {
                    return BadRequest(new { message = "Productnaam is verplicht" });
                }
                
                if (product.SupplierId <= 0)
                {
                    return BadRequest(new { message = "Leverancier is verplicht" });
                }
                
                if (product.CreatedDate == default)
                {
                    product.CreatedDate = DateTime.Now;
                }
                
                // Ensure empty strings for optional fields instead of null
                product.Description ??= string.Empty;
                product.SKU ??= string.Empty;
                product.Category ??= string.Empty;
                
                // Set defaults for numeric fields
                if (product.PurchasePrice <= 0)
                {
                    product.PurchasePrice = 0;
                }
                if (product.SellingPrice <= 0)
                {
                    product.SellingPrice = 0;
                }
                if (product.MinimumStock <= 0)
                {
                    product.MinimumStock = 10;
                }
                
                // Detach navigation properties to prevent EF from trying to insert related entities
                product.Supplier = null;
                
                _context.Products.Add(product);
                await _context.SaveChangesAsync(); // EERST product opslaan zodat ProductId beschikbaar is

                _logger.LogInformation("API: Product {ProductName} (ID: {ProductId}) aangemaakt", 
                    product.ProductName, product.ProductId);

                // CHECK EN MAAK STOCK ALERT AAN indien nodig (NU met correcte ProductId)
                await CheckAndCreateStockAlertAsync(product);

                return CreatedAtAction("GetProduct", new { id = product.ProductId }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: Fout bij aanmaken van product {ProductName}", product.ProductName);
                return StatusCode(500, new { message = $"Fout bij opslaan product: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }

        // DELETE: api/Products/5
        // SOFT DELETE implementatie - consistent met MAUI app
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // SOFT DELETE: markeer als verwijderd in plaats van hard delete
            product.IsDeleted = true;
            product.DeletedDate = DateTime.Now;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id && !e.IsDeleted);
        }

        // =====================================================================
        // HELPER METHODS: Stock Alert Management (Same as Web MVC Controller)
        // =====================================================================
        
        /// <summary>
        /// Controleert voorraad en maakt stock alert aan indien nodig
        /// </summary>
        private async Task CheckAndCreateStockAlertAsync(Product product)
        {
            if (product.StockQuantity >= product.MinimumStock)
            {
                _logger.LogInformation("API: Product {ProductName} (ID: {ProductId}) heeft voldoende voorraad ({Stock} >= {Min}), geen alert nodig", 
                    product.ProductName, product.ProductId, product.StockQuantity, product.MinimumStock);
                return; // Voorraad is OK, geen alert nodig
            }

            try
            {
                _logger.LogInformation("API: Checking for existing alerts for Product {ProductId}", product.ProductId);
                
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

                    _logger.LogInformation("API: ✓ Stock Alert SUCCESVOL aangemaakt voor Product {ProductName} (ID: {ProductId}) - Type: {AlertType}, StockAlertId: {StockAlertId}", 
                        product.ProductName, product.ProductId, alertType, alert.StockAlertId);
                }
                else
                {
                    _logger.LogInformation("API: Stock Alert bestaat al voor Product {ProductName} (ID: {ProductId}) - Alert ID: {AlertId}", 
                        product.ProductName, product.ProductId, existingAlert.StockAlertId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: ✗ FOUT bij aanmaken van stock alert voor Product {ProductName} (ID: {ProductId})", 
                    product.ProductName, product.ProductId);
                // We gooien de exception NIET verder, zodat het aanmaken van het product niet faalt
            }
        }

        /// <summary>
        /// Update stock alerts gebaseerd op voorraadwijzigingen
        /// </summary>
        private async Task CheckAndUpdateStockAlertsAsync(Product product, int previousStock)
        {
            try
            {
                _logger.LogInformation("API: CheckAndUpdateStockAlerts voor Product {ProductId}: Voorraad {OldStock} -> {NewStock}, Minimum: {MinStock}", 
                    product.ProductId, previousStock, product.StockQuantity, product.MinimumStock);

                // Scenario 1: Voorraad is nu ONDER minimum (maak of update alert)
                if (product.StockQuantity < product.MinimumStock)
                {
                    _logger.LogInformation("API: Voorraad ONDER minimum - checking for existing alert");
                    
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
                        
                        _logger.LogInformation("API: ✓ Stock Alert AANGEMAAKT voor Product {ProductName} (ID: {ProductId}) - Type: {AlertType}, AlertId: {AlertId}", 
                            product.ProductName, product.ProductId, alertType, alert.StockAlertId);
                    }
                    else
                    {
                        // Update bestaande alert
                        existingAlert.AlertType = alertType;
                        existingAlert.Notes = $"Voorraad is {product.StockQuantity} stuks, minimum is {product.MinimumStock}. Laatst gewijzigd: {DateTime.Now:dd-MM-yyyy HH:mm}";
                        _context.StockAlerts.Update(existingAlert);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("API: ⟳ Stock Alert GEUPDATE voor Product {ProductName} (ID: {ProductId}) - Type: {AlertType}", 
                            product.ProductName, product.ProductId, alertType);
                    }
                }
                // Scenario 2: Voorraad was ONDER minimum, maar is nu BOVEN minimum (resolve alerts)
                else if (previousStock < product.MinimumStock && product.StockQuantity >= product.MinimumStock)
                {
                    _logger.LogInformation("API: Voorraad HERSTELD - resolving active alerts");
                    
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
                        _logger.LogInformation("API: ✓ {Count} Stock Alert(s) OPGELOST voor Product {ProductName} (ID: {ProductId})", 
                            activeAlerts.Count, product.ProductName, product.ProductId);
                    }
                }
                else
                {
                    _logger.LogInformation("API: Geen stock alert actie nodig voor Product {ProductId}", product.ProductId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API: ✗ FOUT bij updaten van stock alerts voor Product {ProductName} (ID: {ProductId})", 
                    product.ProductName, product.ProductId);
                // We gooien de exception NIET verder, zodat het updaten van het product niet faalt
            }
        }
    }
}
