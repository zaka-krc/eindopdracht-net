using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem_Web.Controllers
{
    public class StockAlertsController : Controller
    {
        private readonly SuntoryDbContext _context;

        public StockAlertsController(SuntoryDbContext context)
        {
            _context = context;
        }

        // GET: StockAlerts
        public async Task<IActionResult> Index()
        {
            var suntoryDbContext = _context.StockAlerts.Include(s => s.Product);
            return View(await suntoryDbContext.ToListAsync());
        }

        // GET: StockAlerts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockAlert = await _context.StockAlerts
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.StockAlertId == id);
            if (stockAlert == null)
            {
                return NotFound();
            }

            return View(stockAlert);
        }

        // GET: StockAlerts/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category");
            return View();
        }

        // POST: StockAlerts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StockAlertId,ProductId,AlertType,Status,CreatedDate,ResolvedDate,Notes,IsDeleted,DeletedDate")] StockAlert stockAlert)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stockAlert);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category", stockAlert.ProductId);
            return View(stockAlert);
        }

        // GET: StockAlerts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockAlert = await _context.StockAlerts.FindAsync(id);
            if (stockAlert == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category", stockAlert.ProductId);
            return View(stockAlert);
        }

        // POST: StockAlerts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StockAlertId,ProductId,AlertType,Status,CreatedDate,ResolvedDate,Notes,IsDeleted,DeletedDate")] StockAlert stockAlert)
        {
            if (id != stockAlert.StockAlertId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockAlert);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockAlertExists(stockAlert.StockAlertId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category", stockAlert.ProductId);
            return View(stockAlert);
        }

        // GET: StockAlerts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockAlert = await _context.StockAlerts
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.StockAlertId == id);
            if (stockAlert == null)
            {
                return NotFound();
            }

            return View(stockAlert);
        }

        // POST: StockAlerts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockAlert = await _context.StockAlerts.FindAsync(id);
            if (stockAlert != null)
            {
                _context.StockAlerts.Remove(stockAlert);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StockAlertExists(int id)
        {
            return _context.StockAlerts.Any(e => e.StockAlertId == id);
        }
    }
}
