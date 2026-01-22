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
    public class CustomersController : ControllerBase
    {
        private readonly SuntoryDbContext _context;

        public CustomersController(SuntoryDbContext context)
        {
            _context = context;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            // Filter soft deleted customers
            return await _context.Customers
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id && !c.IsDeleted);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }

        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            // Detach navigation properties to prevent EF from trying to update related entities
            customer.Deliveries = null;

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
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

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        {
            // Reset identity column for new entities (EF will generate the ID)
            customer.CustomerId = 0;
            
            // Ensure required fields have default values if missing
            if (string.IsNullOrWhiteSpace(customer.CustomerName))
            {
                return BadRequest(new { message = "Klantnaam is verplicht" });
            }
            
            if (customer.CreatedDate == default)
            {
                customer.CreatedDate = DateTime.Now;
            }
            
            if (string.IsNullOrWhiteSpace(customer.CustomerType))
            {
                customer.CustomerType = "Retail";
            }
            
            if (string.IsNullOrWhiteSpace(customer.Status))
            {
                customer.Status = "Active";
            }
            
            // Ensure empty strings for optional fields instead of null
            customer.Address ??= string.Empty;
            customer.PostalCode ??= string.Empty;
            customer.City ??= string.Empty;
            customer.PhoneNumber ??= string.Empty;
            customer.Email ??= string.Empty;
            customer.ContactPerson ??= string.Empty;
            customer.Notes ??= string.Empty;
            
            // Detach navigation properties to prevent EF from trying to insert related entities
            customer.Deliveries = null;
            
            // Validate ModelState before saving
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                var errorMessage = string.Join("; ", errors.Select(e => e.ErrorMessage));
                return BadRequest(new { message = errorMessage });
            }
            
            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetCustomer", new { id = customer.CustomerId }, customer);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Fout bij opslaan klant: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }

        // DELETE: api/Customers/5
        // SOFT DELETE implementatie - consistent met MAUI app
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            // SOFT DELETE: markeer als verwijderd in plaats van hard delete
            customer.IsDeleted = true;
            customer.DeletedDate = DateTime.Now;
            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id && !e.IsDeleted);
        }
    }
}
