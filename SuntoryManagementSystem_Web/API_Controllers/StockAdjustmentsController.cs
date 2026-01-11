using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuntoryManagementSystem.Models;
using SuntoryManagementSystem_Models.Data;

namespace SuntoryManagementSystem_Web.API_Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockAdjustmentsController : ControllerBase
    {
        private readonly SuntoryDbContext _context;

        public StockAdjustmentsController(SuntoryDbContext context)
        {
            _context = context;
        }

        // GET: api/StockAdjustments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockAdjustment>>> GetStockAdjustments()
        {
            return await _context.StockAdjustments.ToListAsync();
        }

        // GET: api/StockAdjustments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StockAdjustment>> GetStockAdjustment(int id)
        {
            var stockAdjustment = await _context.StockAdjustments.FindAsync(id);

            if (stockAdjustment == null)
            {
                return NotFound();
            }

            return stockAdjustment;
        }

        // PUT: api/StockAdjustments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockAdjustment(int id, StockAdjustment stockAdjustment)
        {
            if (id != stockAdjustment.StockAdjustmentId)
            {
                return BadRequest();
            }

            // Detach navigation properties to prevent EF from trying to update related entities
            stockAdjustment.Product = null;

            _context.Entry(stockAdjustment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockAdjustmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/StockAdjustments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StockAdjustment>> PostStockAdjustment(StockAdjustment stockAdjustment)
        {
            // Reset identity column for new entities (EF will generate the ID)
            stockAdjustment.StockAdjustmentId = 0;
            
            // Detach navigation properties to prevent EF from trying to insert related entities
            stockAdjustment.Product = null;
            
            _context.StockAdjustments.Add(stockAdjustment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStockAdjustment", new { id = stockAdjustment.StockAdjustmentId }, stockAdjustment);
        }

        // DELETE: api/StockAdjustments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockAdjustment(int id)
        {
            var stockAdjustment = await _context.StockAdjustments.FindAsync(id);
            if (stockAdjustment == null)
            {
                return NotFound();
            }

            _context.StockAdjustments.Remove(stockAdjustment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockAdjustmentExists(int id)
        {
            return _context.StockAdjustments.Any(e => e.StockAdjustmentId == id);
        }
    }
}
