using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem_Web
{
    public class StockAdjustmentsController : Controller
    {
        private readonly SuntoryDbContext _context;

        public StockAdjustmentsController(SuntoryDbContext context)
        {
            _context = context;
        }

        // GET: StockAdjustments
        public async Task<IActionResult> Index()
        {
            var suntoryDbContext = _context.StockAdjustments.Include(s => s.Product);
            return View(await suntoryDbContext.ToListAsync());
        }

        // GET: StockAdjustments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockAdjustment = await _context.StockAdjustments
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.StockAdjustmentId == id);
            if (stockAdjustment == null)
            {
                return NotFound();
            }

            return View(stockAdjustment);
        }

        // GET: StockAdjustments/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category");
            return View();
        }

        // POST: StockAdjustments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StockAdjustmentId,ProductId,AdjustmentType,QuantityChange,PreviousQuantity,NewQuantity,Reason,AdjustmentDate,AdjustedBy,IsDeleted,DeletedDate")] StockAdjustment stockAdjustment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stockAdjustment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category", stockAdjustment.ProductId);
            return View(stockAdjustment);
        }

        // GET: StockAdjustments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockAdjustment = await _context.StockAdjustments.FindAsync(id);
            if (stockAdjustment == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category", stockAdjustment.ProductId);
            return View(stockAdjustment);
        }

        // POST: StockAdjustments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StockAdjustmentId,ProductId,AdjustmentType,QuantityChange,PreviousQuantity,NewQuantity,Reason,AdjustmentDate,AdjustedBy,IsDeleted,DeletedDate")] StockAdjustment stockAdjustment)
        {
            if (id != stockAdjustment.StockAdjustmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stockAdjustment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StockAdjustmentExists(stockAdjustment.StockAdjustmentId))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category", stockAdjustment.ProductId);
            return View(stockAdjustment);
        }

        // GET: StockAdjustments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockAdjustment = await _context.StockAdjustments
                .Include(s => s.Product)
                .FirstOrDefaultAsync(m => m.StockAdjustmentId == id);
            if (stockAdjustment == null)
            {
                return NotFound();
            }

            return View(stockAdjustment);
        }

        // POST: StockAdjustments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockAdjustment = await _context.StockAdjustments.FindAsync(id);
            if (stockAdjustment != null)
            {
                _context.StockAdjustments.Remove(stockAdjustment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StockAdjustmentExists(int id)
        {
            return _context.StockAdjustments.Any(e => e.StockAdjustmentId == id);
        }
    }
}
