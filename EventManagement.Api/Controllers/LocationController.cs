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
    public class LocationController : ControllerBase
    {
        private readonly EventManagementContext _context;

        public LocationController(EventManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Vraća sve lokacije.
        /// </summary>
        // GET: api/Location
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
        {
            List<Location> locations = await _context.Locations.Include(l => l.Address).ToListAsync();
            return Ok(locations);
        }

        /// <summary>
        /// Vraća specifičnu lokaciju po id-u.
        /// </summary>
        // GET: api/Location/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> GetLocation(int id)
        {
            List<Location> locations = await _context.Locations.Include(l => l.Address).ToListAsync();
            Location l = locations.FirstOrDefault(l => l.LocationId==id);

            if (l == null)
            {
                return NotFound();
            }

            return l;
        }

        /// <summary>
        /// Ažurira specifičnu lokaciju po id-u.
        /// </summary>
        // PUT: api/Location/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, Location location)
        {
            if (id != location.LocationId)
            {
                return BadRequest();
            }

            var l = await _context.Locations.FindAsync(id);
            if (l == null)
            {
                return NotFound();
            }

            l.Venue = location.Venue;

            GetOrCreateAddress(location, l);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!LocationExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Objavljuje lokaciju.
        /// </summary>
        // POST: api/Location
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Location>> CreateLocation(Location location)
        {
            var l = new Location
            {
                Venue = location.Venue
            };

            GetOrCreateAddress(location, l);

            _context.Locations.Add(l);
            await _context.SaveChangesAsync();

            //return CreatedAtAction(
            //    nameof(GetLocation),
            //    new { id = l.LocationId },
            //    l);
            return Ok("Location Created!");
        }

        /// <summary>
        /// Briše specifičnu lokaciju po id-u.
        /// </summary>
        // DELETE: api/Location/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var l = await _context.Locations.FindAsync(id);

            if (l == null)
            {
                return NotFound();
            }

            _context.Locations.Remove(l);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LocationExists(int id)
        {
            return (_context.Locations?.Any(l => l.LocationId == id)).GetValueOrDefault();
        }

        private void GetOrCreateAddress(Location location, Location l)
        {
            List<Address> addresses = _context.Addresses.Where(x =>
            x.City == location.Address.City
            && x.ZipCode == location.Address.ZipCode
            && x.Street == location.Address.Street
            && x.HouseNumber == location.Address.HouseNumber).ToList();

            if (addresses.Count > 0)
            {
                l.Address = addresses[0];
            }
            else
            {
                Address address = new Address
                {
                    City = location.Address.City,
                    HouseNumber = location.Address.HouseNumber,
                    Street = location.Address.Street,
                    ZipCode = location.Address.ZipCode
                };

                _context.Addresses.Add(address);
                _context.SaveChanges();

                addresses = _context.Addresses.Where(x => x.AddressId == address.AddressId).ToList();

                l.Address = addresses[0];
            }
        }
    }
}
