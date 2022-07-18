using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagement.Api.Models;
using EventManagement.Api.Models.DTO;

namespace EventManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly EventManagementContext _context;

        public AddressController(EventManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Vraća sve adrese.
        /// </summary>
        // GET: api/Address
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
        {
            List<Address> addresses = await _context.Addresses.ToListAsync();
            return Ok(addresses);
        }

        /// <summary>
        /// Vraća specifičnu adresu po id-u.
        /// </summary>
        // GET: api/Address/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddress(int id)
        {
            var a = await _context.Addresses.FindAsync(id);

            if (a == null)
            {
                return NotFound();
            }

            return a;
        }

        /// <summary>
        /// Ažurira specifičnu adresu po id-u.
        /// </summary>
        // PUT: api/Address/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, Address address)
        {
            if (id != address.AddressId)
            {
                return BadRequest();
            }

            var a = await _context.Addresses.FindAsync(id);
            if (a == null)
            {
                return NotFound();
            }

            a.City=address.City;
            a.ZipCode=address.ZipCode;
            a.Street = address.Street;
            a.HouseNumber=address.HouseNumber;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!AddressExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Objavljuje adresu.
        /// </summary>
        // POST: api/Address
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Address>> CreateAddress(Address address)
        {
            _context.Addresses.Add(address);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetAddress),
                new { id = address.AddressId },
                address);
        }

        /// <summary>
        /// Briše specifičnu adresu po id-u.
        /// </summary>
        // DELETE: api/Address/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var a = await _context.Addresses.FindAsync(id);

            if (a == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(a);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AddressExists(int id)
        {
            return (_context.Addresses?.Any(a => a.AddressId == id)).GetValueOrDefault();
        }
    }
}
