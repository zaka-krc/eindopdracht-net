using Microsoft.AspNetCore.Mvc;
using SuntoryManagementSystem_Web.Models;
using System.Diagnostics;
using SuntoryManagementSystem_Models.Data;
using Microsoft.EntityFrameworkCore;

namespace SuntoryManagementSystem_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SuntoryDbContext _context;

        public HomeController(ILogger<HomeController> logger, SuntoryDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Actieve Klanten (status = Active en niet deleted)
            ViewBag.ActiveCustomers = await _context.Customers
                .CountAsync(c => !c.IsDeleted && c.Status == "Active");

            // Producten (niet deleted)
            ViewBag.TotalProducts = await _context.Products
                .CountAsync(p => !p.IsDeleted);

            // Leveringen (status = Gepland)
            ViewBag.PendingDeliveries = await _context.Deliveries
                .CountAsync(d => !d.IsDeleted && d.Status == "Gepland");

            // Stock Alerts (status = Active)
            ViewBag.StockAlerts = await _context.StockAlerts
                .CountAsync(sa => !sa.IsDeleted && sa.Status == "Active");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
