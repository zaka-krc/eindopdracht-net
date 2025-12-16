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
    public class DeliveryItemsController : Controller
    {
        private readonly SuntoryDbContext _context;

        public DeliveryItemsController(SuntoryDbContext context)
        {
            _context = context;
        }

        // GET: DeliveryItems
        public async Task<IActionResult> Index()
        {
            var suntoryDbContext = _context.DeliveryItems.Include(d => d.Delivery).Include(d => d.Product);
            return View(await suntoryDbContext.ToListAsync());
        }

        // GET: DeliveryItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryItem = await _context.DeliveryItems
                .Include(d => d.Delivery)
                .Include(d => d.Product)
                .FirstOrDefaultAsync(m => m.DeliveryItemId == id);
            if (deliveryItem == null)
            {
                return NotFound();
            }

            return View(deliveryItem);
        }

        // GET: DeliveryItems/Create
        public IActionResult Create()
        {
            ViewData["DeliveryId"] = new SelectList(_context.Deliveries, "DeliveryId", "DeliveryType");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category");
            return View();
        }

        // POST: DeliveryItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DeliveryItemId,DeliveryId,ProductId,Quantity,UnitPrice,IsProcessed,IsDeleted,DeletedDate")] DeliveryItem deliveryItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(deliveryItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DeliveryId"] = new SelectList(_context.Deliveries, "DeliveryId", "DeliveryType", deliveryItem.DeliveryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category", deliveryItem.ProductId);
            return View(deliveryItem);
        }

        // GET: DeliveryItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryItem = await _context.DeliveryItems.FindAsync(id);
            if (deliveryItem == null)
            {
                return NotFound();
            }
            ViewData["DeliveryId"] = new SelectList(_context.Deliveries, "DeliveryId", "DeliveryType", deliveryItem.DeliveryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category", deliveryItem.ProductId);
            return View(deliveryItem);
        }

        // POST: DeliveryItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DeliveryItemId,DeliveryId,ProductId,Quantity,UnitPrice,IsProcessed,IsDeleted,DeletedDate")] DeliveryItem deliveryItem)
        {
            if (id != deliveryItem.DeliveryItemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(deliveryItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeliveryItemExists(deliveryItem.DeliveryItemId))
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
            ViewData["DeliveryId"] = new SelectList(_context.Deliveries, "DeliveryId", "DeliveryType", deliveryItem.DeliveryId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Category", deliveryItem.ProductId);
            return View(deliveryItem);
        }

        // GET: DeliveryItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliveryItem = await _context.DeliveryItems
                .Include(d => d.Delivery)
                .Include(d => d.Product)
                .FirstOrDefaultAsync(m => m.DeliveryItemId == id);
            if (deliveryItem == null)
            {
                return NotFound();
            }

            return View(deliveryItem);
        }

        // POST: DeliveryItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deliveryItem = await _context.DeliveryItems.FindAsync(id);
            if (deliveryItem != null)
            {
                _context.DeliveryItems.Remove(deliveryItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeliveryItemExists(int id)
        {
            return _context.DeliveryItems.Any(e => e.DeliveryItemId == id);
        }
    }
}
