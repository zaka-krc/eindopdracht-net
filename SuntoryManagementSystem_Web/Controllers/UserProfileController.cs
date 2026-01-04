using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SuntoryManagementSystem.Models;

namespace SuntoryManagementSystem_Web.Controllers
{
    /// <summary>
    /// User Profile Controller
    /// Toegankelijk voor: Alle ingelogde gebruikers
    /// </summary>
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(
            UserManager<ApplicationUser> userManager,
            ILogger<UserProfileController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        // =====================================================================
        // GET: UserProfile/Index - Eigen profiel bekijken
        // =====================================================================
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("Gebruiker niet gevonden voor profiel");
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);

                var viewModel = new UserProfileViewModel
                {
                    User = user,
                    Roles = roles
                };

                _logger.LogInformation("Profiel bekeken door gebruiker {UserName} (ID: {UserId})",
                    user.FullName, user.Id);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij laden van gebruikersprofiel");
                TempData["ErrorMessage"] = "Er is een fout opgetreden bij het laden van uw profiel.";
                return RedirectToAction("Index", "Home");
            }
        }
    }

    // =====================================================================
    // VIEW MODEL
    // =====================================================================
    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
