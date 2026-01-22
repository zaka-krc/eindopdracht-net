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
    public class DeliveriesController : ControllerBase
    {
        private readonly SuntoryDbContext _context;

        public DeliveriesController(SuntoryDbContext context)
        {
            _context = context;
        }

        // GET: api/Deliveries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Delivery>>> GetDeliveries()
        {
            // Filter soft deleted deliveries
            return await _context.Deliveries
                .Where(d => !d.IsDeleted)
                .ToListAsync();
        }

        // GET: api/Deliveries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Delivery>> GetDelivery(int id)
        {
            var delivery = await _context.Deliveries
                .FirstOrDefaultAsync(d => d.DeliveryId == id && !d.IsDeleted);

            if (delivery == null)
            {
                return NotFound();
            }

            return delivery;
        }

        // PUT: api/Deliveries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDelivery(int id, Delivery delivery)
        {
            if (id != delivery.DeliveryId)
            {
                return BadRequest();
            }

            // Detach navigation properties to prevent EF from trying to update related entities
            delivery.Supplier = null;
            delivery.Customer = null;
            delivery.Vehicle = null;
            delivery.DeliveryItems = null;

            _context.Entry(delivery).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeliveryExists(id))
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

        // POST: api/Deliveries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Delivery>> PostDelivery(Delivery delivery)
        {
            // Reset identity column for new entities (EF will generate the ID)
            delivery.DeliveryId = 0;
            
            // Ensure required fields have default values if missing
            if (string.IsNullOrWhiteSpace(delivery.ReferenceNumber))
            {
                return BadRequest(new { message = "Referentienummer is verplicht" });
            }
            
            if (string.IsNullOrWhiteSpace(delivery.DeliveryType))
            {
                delivery.DeliveryType = "Outgoing";
            }
            
            if (string.IsNullOrWhiteSpace(delivery.Status))
            {
                delivery.Status = "Gepland";
            }
            
            if (delivery.CreatedDate == default)
            {
                delivery.CreatedDate = DateTime.Now;
            }
            
            // Ensure empty strings for optional fields instead of null
            delivery.Notes ??= string.Empty;
            
            // Detach navigation properties to prevent EF from trying to insert related entities
            delivery.Supplier = null;
            delivery.Customer = null;
            delivery.Vehicle = null;
            delivery.DeliveryItems = null;
            
            try
            {
                _context.Deliveries.Add(delivery);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetDelivery", new { id = delivery.DeliveryId }, delivery);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Fout bij opslaan levering: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }

        // DELETE: api/Deliveries/5
        // SOFT DELETE implementatie - consistent met MAUI app
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDelivery(int id)
        {
            var delivery = await _context.Deliveries.FindAsync(id);
            if (delivery == null)
            {
                return NotFound();
            }

            // SOFT DELETE: markeer als verwijderd in plaats van hard delete
            delivery.IsDeleted = true;
            delivery.DeletedDate = DateTime.Now;
            _context.Deliveries.Update(delivery);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DeliveryExists(int id)
        {
            return _context.Deliveries.Any(e => e.DeliveryId == id && !e.IsDeleted);
        }
    }
}
