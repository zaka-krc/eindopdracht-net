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
    public class DeliveryItemsController : ControllerBase
    {
        private readonly SuntoryDbContext _context;

        public DeliveryItemsController(SuntoryDbContext context)
        {
            _context = context;
        }

        // GET: api/DeliveryItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeliveryItem>>> GetDeliveryItems()
        {
            return await _context.DeliveryItems.ToListAsync();
        }

        // GET: api/DeliveryItems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DeliveryItem>> GetDeliveryItem(int id)
        {
            var deliveryItem = await _context.DeliveryItems.FindAsync(id);

            if (deliveryItem == null)
            {
                return NotFound();
            }

            return deliveryItem;
        }

        // PUT: api/DeliveryItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDeliveryItem(int id, DeliveryItem deliveryItem)
        {
            if (id != deliveryItem.DeliveryItemId)
            {
                return BadRequest();
            }

            // Detach navigation properties to prevent EF from trying to update related entities
            deliveryItem.Delivery = null;
            deliveryItem.Product = null;

            _context.Entry(deliveryItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeliveryItemExists(id))
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

        // POST: api/DeliveryItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<DeliveryItem>> PostDeliveryItem(DeliveryItem deliveryItem)
        {
            // Reset identity column for new entities (EF will generate the ID)
            deliveryItem.DeliveryItemId = 0;
            
            // Detach navigation properties to prevent EF from trying to insert related entities
            deliveryItem.Delivery = null;
            deliveryItem.Product = null;
            
            _context.DeliveryItems.Add(deliveryItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDeliveryItem", new { id = deliveryItem.DeliveryItemId }, deliveryItem);
        }

        // DELETE: api/DeliveryItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeliveryItem(int id)
        {
            var deliveryItem = await _context.DeliveryItems.FindAsync(id);
            if (deliveryItem == null)
            {
                return NotFound();
            }

            _context.DeliveryItems.Remove(deliveryItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DeliveryItemExists(int id)
        {
            return _context.DeliveryItems.Any(e => e.DeliveryItemId == id);
        }
    }
}
