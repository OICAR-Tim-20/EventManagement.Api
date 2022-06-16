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
    public class UserController : ControllerBase
    {
        private readonly EventManagementContext _context;

        public UserController(EventManagementContext context)
        {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IList<UserDTO>>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            IEnumerable<User> users = await _context.Users.ToListAsync();
            IList<UserDTO> userDTOs = new List<UserDTO>();
            foreach (var user in users)
            {
                var userDTO = new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Address = { 
                        AddressId = user.Address.AddressId,
                        City = user.Address.City,
                        Street = user.Address.Street,
                        HouseNumber = user.Address.HouseNumber,
                        ZipCode = user.Address.ZipCode
                    },
                    Picture = user.Picture,
                    ContactName = user.ContactName,
                    PhoneNumber = user.PhoneNumber
                };
                userDTOs.Add(userDTO);
            }
            return Ok(userDTOs);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var @User = await _context.Users.FindAsync(id);

            if (@User == null)
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<string>> PutUser(int id, UserDTO userDTO)
        {
            if (id != userDTO.UserId)
            {
                return BadRequest("Parameter UserId does not match User object UserId (request body).");
            }

            var dbUser = await _context.Users.FindAsync(id);
            
            dbUser.Username = userDTO.Username;
            dbUser.Email = userDTO.Email;
            //dbUser.Address = userDTO.Address;
            dbUser.Picture = userDTO.Picture;
            dbUser.ContactName = userDTO.ContactName;
            dbUser.PhoneNumber = userDTO.PhoneNumber;

            var existingAddress = _context.Addresses.Where(a => a.City == userDTO.Address.City &&
            a.ZipCode == userDTO.Address.ZipCode && a.Street == userDTO.Address.Street
            && a.HouseNumber == userDTO.Address.HouseNumber).FirstOrDefault();

            if (existingAddress != null) {
                dbUser.AddressId = existingAddress.AddressId;
                dbUser.Address = existingAddress;
            }
            else
            {
                Address address = new Address
                {
                    City = userDTO.Address.City,
                    ZipCode = userDTO.Address.ZipCode,
                    Street = userDTO.Address.Street,
                    HouseNumber = userDTO.Address.HouseNumber
                };
                _context.Addresses.Add(address);
                await _context.SaveChangesAsync();
                dbUser.AddressId = address.AddressId;
            }
            

            _context.Entry(dbUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("User profile updated!");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User @User)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'UserManagementContext.Users'  is null.");
            }
            _context.Users.Add(@User);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = @User.UserId }, @User);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var @User = await _context.Users.FindAsync(id);
            if (@User == null)
            {
                return NotFound();
            }

            _context.Users.Remove(@User);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
