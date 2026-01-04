using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem_Web.Controllers
{
    /// <summary>
    /// Admin Controller voor gebruikersbeheer
    /// Toegankelijk voor: ALLEEN Administrator rol
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly SuntoryDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            SuntoryDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // =====================================================================
        // GET: Admin/Users - Alle gebruikers lijst
        // =====================================================================
        public async Task<IActionResult> Users()
        {
            try
            {
                _logger.LogInformation("Admin Users pagina bezocht door {User}", User.Identity?.Name ?? "Anonymous");

                var users = await _context.Users
                    .OrderBy(u => u.IsActive ? 0 : 1)  // Actieve eerst
                    .ThenBy(u => u.FullName)
                    .ToListAsync();

                // Haal rollen op voor elke gebruiker
                var userViewModels = new List<UserViewModel>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userViewModels.Add(new UserViewModel
                    {
                        User = user,
                        Roles = roles.ToList()
                    });
                }

                return View(userViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van gebruikers");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de gebruikers.";
                return View(new List<UserViewModel>());
            }
        }

        // =====================================================================
        // GET: Admin/UserDetails/id - Details van specifieke gebruiker
        // =====================================================================
        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("UserDetails aangeroepen zonder ID");
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Gebruiker met ID {UserId} niet gevonden", id);
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);

                var viewModel = new UserViewModel
                {
                    User = user,
                    Roles = roles.ToList()
                };

                _logger.LogInformation("Details van gebruiker {UserName} (ID: {UserId}) bekeken door {Admin}",
                    user.FullName, id, User.Identity?.Name ?? "Anonymous");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van gebruiker details voor ID {UserId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de gebruiker details.";
                return RedirectToAction(nameof(Users));
            }
        }

        // =====================================================================
        // GET: Admin/ManageRoles/id - Beheer rollen voor gebruiker
        // =====================================================================
        public async Task<IActionResult> ManageRoles(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("ManageRoles aangeroepen zonder ID");
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Gebruiker met ID {UserId} niet gevonden", id);
                    return NotFound();
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = await _roleManager.Roles.ToListAsync();

                var viewModel = new ManageRolesViewModel
                {
                    UserId = user.Id,
                    UserName = user.FullName,
                    UserEmail = user.Email ?? "",
                    AllRoles = allRoles.Select(r => new RoleSelection
                    {
                        RoleName = r.Name ?? "",
                        IsSelected = userRoles.Contains(r.Name ?? "")
                    }).ToList()
                };

                _logger.LogInformation("ManageRoles pagina geopend voor gebruiker {UserName} (ID: {UserId}) door {Admin}",
                    user.FullName, id, User.Identity?.Name ?? "Anonymous");

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen van rollen voor gebruiker ID {UserId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van de rollen.";
                return RedirectToAction(nameof(Users));
            }
        }

        // =====================================================================
        // POST: Admin/ManageRoles - Update rollen voor gebruiker
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageRolesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Ongeldige gegevens.";
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Gebruiker met ID {UserId} niet gevonden", model.UserId);
                    return NotFound();
                }

                // Haal huidige rollen op
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Verwijder alle huidige rollen
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Fout bij verwijderen van oude rollen.";
                    return View(model);
                }

                // Voeg nieuwe geselecteerde rollen toe
                var selectedRoles = model.AllRoles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();
                if (selectedRoles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
                    if (!addResult.Succeeded)
                    {
                        TempData["ErrorMessage"] = "Fout bij toevoegen van nieuwe rollen.";
                        return View(model);
                    }
                }

                _logger.LogInformation("Rollen bijgewerkt voor gebruiker {UserName} (ID: {UserId}) door {Admin} - Nieuwe rollen: {Roles}",
                    user.FullName, user.Id, User.Identity?.Name ?? "Anonymous", string.Join(", ", selectedRoles));

                TempData["SuccessMessage"] = $"Rollen voor '{user.FullName}' succesvol bijgewerkt!";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij updaten van rollen voor gebruiker ID {UserId}", model.UserId);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het bijwerken van de rollen.";
                return View(model);
            }
        }

        // =====================================================================
        // POST: Admin/ToggleActiveStatus - Activeer/Deactiveer gebruiker
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActiveStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Geen gebruiker ID opgegeven.";
                return RedirectToAction(nameof(Users));
            }

            // Voorkom dat admin zichzelf deactiveert
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["ErrorMessage"] = "U kunt uw eigen account niet deactiveren!";
                return RedirectToAction(nameof(Users));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Gebruiker met ID {UserId} niet gevonden", id);
                    TempData["ErrorMessage"] = "Gebruiker niet gevonden.";
                    return RedirectToAction(nameof(Users));
                }

                user.IsActive = !user.IsActive;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    string action = user.IsActive ? "geactiveerd" : "gedeactiveerd";
                    _logger.LogInformation("Gebruiker {UserName} (ID: {UserId}) {Action} door {Admin}",
                        user.FullName, user.Id, action, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Gebruiker '{user.FullName}' succesvol {action}!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Fout bij wijzigen van gebruikersstatus.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij wijzigen van status voor gebruiker ID {UserId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het wijzigen van de gebruikersstatus.";
            }

            return RedirectToAction(nameof(Users));
        }

        // =====================================================================
        // POST: Admin/ResetPassword - Reset wachtwoord naar standaard
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Geen gebruiker ID opgegeven.";
                return RedirectToAction(nameof(Users));
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("Gebruiker met ID {UserId} niet gevonden", id);
                    TempData["ErrorMessage"] = "Gebruiker niet gevonden.";
                    return RedirectToAction(nameof(Users));
                }

                // Verwijder oude wachtwoord
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (!removePasswordResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Fout bij verwijderen van oud wachtwoord.";
                    return RedirectToAction(nameof(Users));
                }

                // Voeg nieuw standaard wachtwoord toe
                var addPasswordResult = await _userManager.AddPasswordAsync(user, "Reset@123");
                if (addPasswordResult.Succeeded)
                {
                    _logger.LogInformation("Wachtwoord gereset voor gebruiker {UserName} (ID: {UserId}) door {Admin}",
                        user.FullName, user.Id, User.Identity?.Name ?? "Anonymous");

                    TempData["SuccessMessage"] = $"Wachtwoord voor '{user.FullName}' succesvol gereset naar: Reset@123";
                }
                else
                {
                    TempData["ErrorMessage"] = "Fout bij instellen van nieuw wachtwoord: " + 
                        string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij resetten van wachtwoord voor gebruiker ID {UserId}", id);
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het resetten van het wachtwoord.";
            }

            return RedirectToAction(nameof(Users));
        }

        // =====================================================================
        // GET: Admin/Dashboard - Admin overzicht dashboard
        // =====================================================================
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var inactiveUsers = totalUsers - activeUsers;

                var totalAdmins = await _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                    .Where(x => x.r.Name == "Administrator")
                    .CountAsync();

                var totalManagers = await _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                    .Where(x => x.r.Name == "Manager")
                    .CountAsync();

                var totalEmployees = await _context.UserRoles
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur, r })
                    .Where(x => x.r.Name == "Employee")
                    .CountAsync();

                var recentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedDate)
                    .Take(5)
                    .ToListAsync();

                var dashboard = new AdminDashboardViewModel
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    InactiveUsers = inactiveUsers,
                    TotalAdministrators = totalAdmins,
                    TotalManagers = totalManagers,
                    TotalEmployees = totalEmployees,
                    RecentUsers = recentUsers
                };

                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij laden van admin dashboard");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van het dashboard.";
                return View(new AdminDashboardViewModel());
            }
        }
    }

    // =====================================================================
    // VIEW MODELS
    // =====================================================================

    public class UserViewModel
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class ManageRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<RoleSelection> AllRoles { get; set; } = new List<RoleSelection>();
    }

    public class RoleSelection
    {
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int TotalAdministrators { get; set; }
        public int TotalManagers { get; set; }
        public int TotalEmployees { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
    }
}
