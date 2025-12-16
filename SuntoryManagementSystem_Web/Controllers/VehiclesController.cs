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
    /// Controller voor het beheren van Voertuigen (Vehicles)
    /// Implementeert volledige CRUD operaties met authorization, soft delete en logging
    /// </summary>
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly SuntoryDbContext _context;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(SuntoryDbContext context, ILogger<VehiclesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =====================================================================
        // GET: Vehicles
        // Toegankelijk voor: Alle ingelogde gebruikers
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Vehicles Index pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");

                // Filter soft deleted vehicles en sorteer op beschikbaarheid en kenteken
                var vehicles = await _context.Vehicles
                    .Where(v => !v.IsDeleted)
                    .OrderBy(v => v.IsAvailable ? 0 : 1)  // Beschikbare eerst
                    .ThenBy(v => v.LicensePlate)
                    .ToListAsync();

                return View(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van vehicles");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voertuigen.";
                return View(new List<Vehicle>());
            }
        }

        // =====================================================================
        // GET: Vehicles/Details/5
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
                var vehicle = await _context.Vehicles
                    .Where(v => !v.IsDeleted)
                    .FirstOrDefaultAsync(m => m.VehicleId == id);

                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle met ID {VehicleId} niet gevonden", id);
                    return NotFound();
                }

                _logger.LogInformation("Details van Vehicle {LicensePlate} (ID: {VehicleId}) bekeken door {User}", 
                    vehicle.LicensePlate, id, User.Identity?.Name ?? "Anonymous");

                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van vehicle details voor ID {VehicleId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de voertuig details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // GET: Vehicles/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            _logger.LogInformation("Create pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");
            return View();
        }

        // =====================================================================
        // POST: Vehicles/Create
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Create([Bind("VehicleId,LicensePlate,Brand,Model,VehicleType,Capacity,IsAvailable,LastMaintenanceDate,Notes")] Vehicle vehicle)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Zet created date
                    vehicle.CreatedDate = DateTime.Now;
                    vehicle.IsDeleted = false;

                    _context.Add(vehicle);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Vehicle {LicensePlate} (ID: {VehicleId}) aangemaakt door {User}", 
                        vehicle.LicensePlate, vehicle.VehicleId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Voertuig '{vehicle.LicensePlate}' succesvol toegevoegd!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij aanmaken van vehicle {LicensePlate}", vehicle.LicensePlate);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het aanmaken van het voertuig.";
            }

            return View(vehicle);
        }

        // =====================================================================
        // GET: Vehicles/Edit/5
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
                var vehicle = await _context.Vehicles
                    .Where(v => !v.IsDeleted)
                    .FirstOrDefaultAsync(v => v.VehicleId == id);

                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle met ID {VehicleId} niet gevonden voor edit", id);
                    return NotFound();
                }

                _logger.LogInformation("Edit pagina geopend voor Vehicle {LicensePlate} (ID: {VehicleId}) door {User}", 
                    vehicle.LicensePlate, id, User.Identity?.Name ?? "Anonymous");

                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van vehicle voor edit met ID {VehicleId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van het voertuig.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Vehicles/Edit/5
        // Toegankelijk voor: Administrator, Manager
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("VehicleId,LicensePlate,Brand,Model,VehicleType,Capacity,IsAvailable,LastMaintenanceDate,CreatedDate,Notes,IsDeleted,DeletedDate")] Vehicle vehicle)
        {
            if (id != vehicle.VehicleId)
            {
                _logger.LogWarning("ID mismatch in Edit: URL ID {UrlId} vs Vehicle ID {VehicleId}", id, vehicle.VehicleId);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehicle);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Vehicle {LicensePlate} (ID: {VehicleId}) gewijzigd door {User}", 
                        vehicle.LicensePlate, vehicle.VehicleId, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Voertuig '{vehicle.LicensePlate}' succesvol gewijzigd!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!VehicleExists(vehicle.VehicleId))
                    {
                        _logger.LogWarning("Vehicle met ID {VehicleId} niet meer gevonden tijdens update", vehicle.VehicleId);
                        TempData["ErrorMessage"] = "Het voertuig bestaat niet meer in de database.";
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency fout bij wijzigen van vehicle {VehicleId}", vehicle.VehicleId);
                        TempData["ErrorMessage"] = "Er is een conflict opgetreden. Mogelijk is het voertuig al gewijzigd door een andere gebruiker.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij wijzigen van vehicle {VehicleId}", vehicle.VehicleId);
                    TempData["ErrorMessage"] = "Er is een fout opgetreden bij het wijzigen van het voertuig.";
                }
            }

            return View(vehicle);
        }

        // =====================================================================
        // GET: Vehicles/Delete/5
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
                var vehicle = await _context.Vehicles
                    .Where(v => !v.IsDeleted)
                    .FirstOrDefaultAsync(m => m.VehicleId == id);

                if (vehicle == null)
                {
                    _logger.LogWarning("Vehicle met ID {VehicleId} niet gevonden voor delete", id);
                    return NotFound();
                }

                _logger.LogInformation("Delete confirmatie pagina geopend voor Vehicle {LicensePlate} (ID: {VehicleId}) door {User}", 
                    vehicle.LicensePlate, id, User.Identity?.Name ?? "Anonymous");

                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van vehicle voor delete met ID {VehicleId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van het voertuig.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // POST: Vehicles/Delete/5
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
                var vehicle = await _context.Vehicles.FindAsync(id);

                if (vehicle != null)
                {
                    // SOFT DELETE: markeer als verwijderd in plaats van hard delete
                    vehicle.IsDeleted = true;
                    vehicle.DeletedDate = DateTime.Now;
                    _context.Vehicles.Update(vehicle);

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Vehicle {LicensePlate} (ID: {VehicleId}) soft deleted door {User}", 
                        vehicle.LicensePlate, id, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Voertuig '{vehicle.LicensePlate}' succesvol verwijderd!";
                }
                else
                {
                    _logger.LogWarning("Vehicle met ID {VehicleId} niet gevonden voor delete", id);
                    TempData["ErrorMessage"] = "Het voertuig kon niet worden gevonden.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwijderen van vehicle met ID {VehicleId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het verwijderen van het voertuig.";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================================
        // HELPER METHODE: Check of vehicle bestaat
        // =====================================================================
        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.VehicleId == id && !e.IsDeleted);
        }
    }
}
