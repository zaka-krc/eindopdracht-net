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
            
            // Add products list for delivery items
            ViewBag.Products = _context.Products
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.ProductName)
                .Select(p => new { 
                    p.ProductId, 
                    p.ProductName, 
                    p.PurchasePrice, 
                    p.SellingPrice,
                    p.StockQuantity 
                })
                .ToList();
            
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
                // Remove validation errors for supplier/customer based on delivery type
                if (delivery.DeliveryType == "Incoming")
                {
                    ModelState.Remove("CustomerId");
                    delivery.CustomerId = null;
                }
                else if (delivery.DeliveryType == "Outgoing")
                {
                    ModelState.Remove("SupplierId");
                    delivery.SupplierId = null;
                }

                if (ModelState.IsValid)
                {
                    // Zet created date
                    delivery.CreatedDate = DateTime.Now;
                    delivery.IsDeleted = false;

                    _context.Add(delivery);
                    await _context.SaveChangesAsync();

                    // Process delivery items from form
                    var deliveryItemsList = new List<DeliveryItem>();
                    int itemIndex = 0;
                    
                    while (Request.Form.ContainsKey($"DeliveryItems[{itemIndex}].ProductId"))
                    {
                        var productIdStr = Request.Form[$"DeliveryItems[{itemIndex}].ProductId"].ToString();
                        var quantityStr = Request.Form[$"DeliveryItems[{itemIndex}].Quantity"].ToString();
                        var unitPriceStr = Request.Form[$"DeliveryItems[{itemIndex}].UnitPrice"].ToString();

                        if (int.TryParse(productIdStr, out int productId) &&
                            int.TryParse(quantityStr, out int quantity) &&
                            decimal.TryParse(unitPriceStr, out decimal unitPrice))
                        {
                            var deliveryItem = new DeliveryItem
                            {
                                DeliveryId = delivery.DeliveryId,
                                ProductId = productId,
                                Quantity = quantity,
                                UnitPrice = unitPrice,
                                IsProcessed = false
                            };
                            
                            deliveryItemsList.Add(deliveryItem);
                        }
                        
                        itemIndex++;
                    }

                    // Save delivery items
                    if (deliveryItemsList.Any())
                    {
                        _context.DeliveryItems.AddRange(deliveryItemsList);
                        await _context.SaveChangesAsync();
                        
                        _logger.LogInformation("Delivery {ReferenceNumber} (ID: {DeliveryId}) aangemaakt met {ItemCount} items door {User}",
                            delivery.ReferenceNumber, delivery.DeliveryId, deliveryItemsList.Count, User.Identity?.Name ?? "Anonymous");
                    }
                    else
                    {
                        _logger.LogInformation("Delivery {ReferenceNumber} (ID: {DeliveryId}) aangemaakt zonder items door {User}",
                            delivery.ReferenceNumber, delivery.DeliveryId, User.Identity?.Name ?? "Anonymous");
                    }

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
            
            // Re-add products list
            ViewBag.Products = _context.Products
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.ProductName)
                .Select(p => new { 
                    p.ProductId, 
                    p.ProductName, 
                    p.PurchasePrice, 
                    p.SellingPrice,
                    p.StockQuantity 
                })
                .ToList();
            
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
                    .Include(d => d.DeliveryItems.Where(di => !di.IsDeleted))
                        .ThenInclude(di => di.Product)
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
                
                // Add products list for delivery items
                ViewBag.Products = _context.Products
                    .Where(p => !p.IsDeleted && p.IsActive)
                    .OrderBy(p => p.ProductName)
                    .Select(p => new { 
                        p.ProductId, 
                        p.ProductName, 
                        p.PurchasePrice, 
                        p.SellingPrice,
                        p.StockQuantity 
                    })
                    .ToList();
                
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

            // Check if delivery is processed or cancelled
            var existingDelivery = await _context.Deliveries.AsNoTracking()
                .FirstOrDefaultAsync(d => d.DeliveryId == id);
            
            if (existingDelivery != null && (existingDelivery.IsProcessed || existingDelivery.Status == "Geannuleerd"))
            {
                TempData["ErrorMessage"] = "Deze levering kan niet meer worden bewerkt omdat deze al " + 
                    (existingDelivery.IsProcessed ? "verwerkt" : "geannuleerd") + " is.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Remove validation errors for supplier/customer based on delivery type
            if (delivery.DeliveryType == "Incoming")
            {
                ModelState.Remove("CustomerId");
                delivery.CustomerId = null;
            }
            else if (delivery.DeliveryType == "Outgoing")
            {
                ModelState.Remove("SupplierId");
                delivery.SupplierId = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update delivery
                    _context.Update(delivery);

                    // Remove old delivery items
                    var existingItems = await _context.DeliveryItems
                        .Where(di => di.DeliveryId == id)
                        .ToListAsync();
                    
                    _context.DeliveryItems.RemoveRange(existingItems);
                    
                    // Add new delivery items from form
                    var deliveryItemsList = new List<DeliveryItem>();
                    int itemIndex = 0;
                    
                    while (Request.Form.ContainsKey($"DeliveryItems[{itemIndex}].ProductId"))
                    {
                        var productIdStr = Request.Form[$"DeliveryItems[{itemIndex}].ProductId"].ToString();
                        var quantityStr = Request.Form[$"DeliveryItems[{itemIndex}].Quantity"].ToString();
                        var unitPriceStr = Request.Form[$"DeliveryItems[{itemIndex}].UnitPrice"].ToString();

                        if (int.TryParse(productIdStr, out int productId) &&
                            int.TryParse(quantityStr, out int quantity) &&
                            decimal.TryParse(unitPriceStr, out decimal unitPrice))
                        {
                            var deliveryItem = new DeliveryItem
                            {
                                DeliveryId = delivery.DeliveryId,
                                ProductId = productId,
                                Quantity = quantity,
                                UnitPrice = unitPrice,
                                IsProcessed = false
                            };
                            
                            deliveryItemsList.Add(deliveryItem);
                        }
                        
                        itemIndex++;
                    }

                    // Save delivery items
                    if (deliveryItemsList.Any())
                    {
                        _context.DeliveryItems.AddRange(deliveryItemsList);
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Delivery {ReferenceNumber} (ID: {DeliveryId}) gewijzigd met {ItemCount} items door {User}",
                        delivery.ReferenceNumber, delivery.DeliveryId, deliveryItemsList.Count, User.Identity?.Name ?? "Anonymous");

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
            
            // Re-add products list
            ViewBag.Products = _context.Products
                .Where(p => !p.IsDeleted && p.IsActive)
                .OrderBy(p => p.ProductName)
                .Select(p => new { 
                    p.ProductId, 
                    p.ProductName, 
                    p.PurchasePrice, 
                    p.SellingPrice,
                    p.StockQuantity 
                })
                .ToList();
            
            // Reload delivery with items for display
            delivery = await _context.Deliveries
                .Include(d => d.DeliveryItems.Where(di => !di.IsDeleted))
                    .ThenInclude(di => di.Product)
                .FirstOrDefaultAsync(d => d.DeliveryId == id);
            
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
        // POST: Deliveries/Process/5
        // Verwerk een levering en update voorraad + stock alerts
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Process(int id)
        {
            try
            {
                var delivery = await _context.Deliveries
                    .Include(d => d.Supplier)
                    .Include(d => d.Customer)
                    .FirstOrDefaultAsync(d => d.DeliveryId == id && !d.IsDeleted);

                if (delivery == null)
                {
                    _logger.LogWarning("Delivery met ID {DeliveryId} niet gevonden voor processing", id);
                    TempData["ErrorMessage"] = "De levering kon niet worden gevonden.";
                    return RedirectToAction(nameof(Index));
                }

                // Check of al verwerkt
                if (delivery.IsProcessed)
                {
                    TempData["ErrorMessage"] = "Deze levering is al verwerkt!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Check of geannuleerd
                if (delivery.Status == "Geannuleerd")
                {
                    TempData["ErrorMessage"] = "Geannuleerde leveringen kunnen niet verwerkt worden!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Haal delivery items op
                var deliveryItems = await _context.DeliveryItems
                    .Include(di => di.Product)
                        .ThenInclude(p => p.Supplier)
                    .Where(di => di.DeliveryId == id && !di.IsDeleted)
                    .ToListAsync();

                if (!deliveryItems.Any())
                {
                    TempData["ErrorMessage"] = "Deze levering heeft geen items!";
                    return RedirectToAction(nameof(Details), new { id });
                }

                bool isIncoming = delivery.DeliveryType == "Incoming";

                // VALIDATIE VOOR OUTGOING LEVERINGEN
                if (!isIncoming)
                {
                    var validationErrors = new System.Text.StringBuilder();

                    foreach (var item in deliveryItems)
                    {
                        if (item.Product == null) continue;

                        if (item.Product.StockQuantity < item.Quantity)
                        {
                            validationErrors.AppendLine(
                                $"- {item.Product.ProductName}: Beschikbaar {item.Product.StockQuantity}, Nodig {item.Quantity}");
                        }
                    }

                    if (validationErrors.Length > 0)
                    {
                        TempData["ErrorMessage"] = "ONVOLDOENDE VOORRAAD! De volgende producten hebben onvoldoende voorraad: " + 
                            validationErrors.ToString();
                        return RedirectToAction(nameof(Details), new { id });
                    }
                }

                // VERWERK DE LEVERING
                int itemsProcessed = 0;
                var processingDetails = new System.Text.StringBuilder();
                processingDetails.AppendLine($"Levering {delivery.ReferenceNumber} verwerkt. ");

                foreach (var item in deliveryItems)
                {
                    if (item.Product == null) continue;

                    int previousQty = item.Product.StockQuantity;
                    int quantityChange = isIncoming ? item.Quantity : -item.Quantity;
                    int newQty = previousQty + quantityChange;

                    item.Product.StockQuantity = newQty;

                    // Create stock adjustment
                    var adjustment = new StockAdjustment
                    {
                        ProductId = item.ProductId,
                        AdjustmentType = isIncoming ? "Addition" : "Removal",
                        QuantityChange = quantityChange,
                        PreviousQuantity = previousQty,
                        NewQuantity = newQty,
                        Reason = $"{delivery.DeliveryType} levering {delivery.ReferenceNumber} verwerkt",
                        AdjustedBy = User.Identity?.Name ?? "System",
                        AdjustmentDate = DateTime.Now
                    };

                    _context.StockAdjustments.Add(adjustment);

                    // CHECK EN UPDATE STOCK ALERTS
                    await CheckAndUpdateStockAlertsForDelivery(item.Product, previousQty);

                    item.IsProcessed = true;
                    itemsProcessed++;

                    string changeSymbol = isIncoming ? "+" : "-";
                    processingDetails.Append($"{item.Product.ProductName} ({previousQty} {changeSymbol} {Math.Abs(quantityChange)} = {newQty}). ");
                }

                // Update delivery status
                delivery.IsProcessed = true;
                delivery.Status = "Delivered";
                delivery.ActualDeliveryDate = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Levering {ReferenceNumber} (ID: {DeliveryId}) verwerkt door {User} - {ItemCount} items",
                    delivery.ReferenceNumber, delivery.DeliveryId, User.Identity?.Name ?? "Anonymous", itemsProcessed);

                TempData["SuccessMessage"] = $"Levering '{delivery.ReferenceNumber}' succesvol verwerkt! {processingDetails}";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwerken van delivery met ID {DeliveryId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwerken van de levering.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // HELPER METHODS: Stock Alert Management
        // =====================================================================

        /// <summary>
        /// Check en update stock alerts voor een product na levering verwerking
        /// </summary>
        private async Task CheckAndUpdateStockAlertsForDelivery(Product product, int previousStock)
        {
            try
            {
                _logger.LogInformation("CheckAndUpdateStockAlertsForDelivery voor Product {ProductId}: Voorraad {OldStock} -> {NewStock}, Minimum: {MinStock}",
                    product.ProductId, previousStock, product.StockQuantity, product.MinimumStock);

                // Scenario 1: Voorraad is nu ONDER minimum (maak of update alert)
                if (product.StockQuantity < product.MinimumStock)
                {
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
                        _logger.LogInformation("✅ Stock Alert AANGEMAAKT na levering voor Product {ProductName} (ID: {ProductId}) - Type: {AlertType}",
                            product.ProductName, product.ProductId, alertType);
                    }
                    else
                    {
                        // Update bestaande alert
                        existingAlert.AlertType = alertType;
                        existingAlert.Notes = $"Voorraad is {product.StockQuantity} stuks, minimum is {product.MinimumStock}. Bijgewerkt: {DateTime.Now:dd-MM-yyyy HH:mm}";
                        _context.StockAlerts.Update(existingAlert);
                        _logger.LogInformation("🔄 Stock Alert GEUPDATE na levering voor Product {ProductName} (ID: {ProductId}) - Type: {AlertType}",
                            product.ProductName, product.ProductId, alertType);
                    }

                    await _context.SaveChangesAsync();
                }
                // Scenario 2: Voorraad was ONDER minimum, maar is nu BOVEN minimum (resolve alerts)
                else if (previousStock < product.MinimumStock && product.StockQuantity >= product.MinimumStock)
                {
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
                        _logger.LogInformation("✅ {Count} Stock Alert(s) OPGELOST na levering voor Product {ProductName} (ID: {ProductId})",
                            activeAlerts.Count, product.ProductName, product.ProductId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ FOUT bij updaten van stock alerts na levering voor Product {ProductId}", product.ProductId);
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
