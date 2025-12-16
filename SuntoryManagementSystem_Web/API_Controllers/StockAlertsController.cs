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
    public class StockAlertsController : ControllerBase
    {
        private readonly SuntoryDbContext _context;

        public StockAlertsController(SuntoryDbContext context)
        {
            _context = context;
        }

        // GET: api/StockAlerts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockAlert>>> GetStockAlerts()
        {
            return await _context.StockAlerts.ToListAsync();
        }

        // GET: api/StockAlerts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StockAlert>> GetStockAlert(int id)
        {
            var stockAlert = await _context.StockAlerts.FindAsync(id);

            if (stockAlert == null)
            {
                return NotFound();
            }

            return stockAlert;
        }

        // PUT: api/StockAlerts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockAlert(int id, StockAlert stockAlert)
        {
            if (id != stockAlert.StockAlertId)
            {
                return BadRequest();
            }

            _context.Entry(stockAlert).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockAlertExists(id))
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

        // POST: api/StockAlerts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StockAlert>> PostStockAlert(StockAlert stockAlert)
        {
            _context.StockAlerts.Add(stockAlert);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStockAlert", new { id = stockAlert.StockAlertId }, stockAlert);
        }

        // DELETE: api/StockAlerts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockAlert(int id)
        {
            var stockAlert = await _context.StockAlerts.FindAsync(id);
            if (stockAlert == null)
            {
                return NotFound();
            }

            _context.StockAlerts.Remove(stockAlert);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockAlertExists(int id)
        {
            return _context.StockAlerts.Any(e => e.StockAlertId == id);
        }
    }
}
