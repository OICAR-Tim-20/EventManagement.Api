using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagement.Api.Models;
using EventManagement.Api.Models.DTO;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

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
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }



            List<User> users = await _context.Users.ToListAsync();
            IEnumerable<UserDTO> userDTOs = users.Select(x => UserToDTO(x));
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



            var u = await _context.Users.FindAsync(id);

            if (u == null)
            {
                return NotFound();
            }

            return UserToDTO(u);
        }

        // PUT: api/Event/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<string>> UpdateUser(int id, UserDTO userDTO)
        {
            if (id != userDTO.UserId)
            {
                return BadRequest("Parameter UserId does not match User object UserId (request body).");
            }

            var u = await _context.Users.FindAsync(id);
            if (u == null)
            {
                return NotFound();
            }

            u.Username = userDTO.Username;
            u.Email = userDTO.Email;
            u.Picture = userDTO.Picture;
            u.ContactName = userDTO.ContactName;
            u.PhoneNumber = userDTO.PhoneNumber;

            u.PasswordHash = Encoding.UTF8.GetBytes(getHash(userDTO.Password));
            u.PasswordSalt = Encoding.UTF8.GetBytes(getSalt());

            GetOrCreateAddress(userDTO, u);



            _context.Entry(u).State = EntityState.Modified;



            try
            {
                await _context.SaveChangesAsync();
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

            return Ok("User profile updated!");
        }

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser(UserDTO userDTO)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'UserManagementContext.Users' is null.");
            }



            var u = new User
            {
                Username = userDTO.Username,
                Email = userDTO.Email,
                Picture = userDTO.Picture,
                ContactName = userDTO.ContactName,
                PhoneNumber = userDTO.PhoneNumber
            };

            u.PasswordHash = Encoding.UTF8.GetBytes(getHash(userDTO.Password));
            u.PasswordSalt = Encoding.UTF8.GetBytes(getSalt());

            GetOrCreateAddress(userDTO, u);

            _context.Users.Add(u);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetUser),
                new { id = u.UserId },
                UserToDTO(u));
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }



            var u = await _context.Users.FindAsync(id);

            if (u == null)
            {
                return NotFound();
            }

            _context.Users.Remove(u);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }

        private UserDTO UserToDTO(User user)
        {
            UserDTO userDTO = new UserDTO();
            userDTO.UserId = user.UserId;
            userDTO.Username = user.Username;
            userDTO.Email = user.Email;
            userDTO.Address = user.Address;
            userDTO.Picture = user.Picture;
            userDTO.ContactName = user.ContactName;
            userDTO.PhoneNumber = user.PhoneNumber;

            return userDTO;
        }

        private static string getHash(string text)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private static string getSalt()
        {
            byte[] bytes = new byte[128 / 8];
            using (var keyGenerator = RandomNumberGenerator.Create())
            {
                keyGenerator.GetBytes(bytes);
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }

        private void GetOrCreateAddress(UserDTO userDTO, User u)
        {
            List<Address> addresses = _context.Addresses.Where(x =>
            x.City == userDTO.Address.City
            && x.ZipCode == userDTO.Address.ZipCode
            && x.Street == userDTO.Address.Street
            && x.HouseNumber == userDTO.Address.HouseNumber).ToList();

            if (addresses.Count > 0)
            {
                u.Address = addresses[0];
            }
            else
            {
                Address address = new Address
                {
                    City = userDTO.Address.City,
                    HouseNumber = userDTO.Address.HouseNumber,
                    Street = userDTO.Address.Street,
                    ZipCode = userDTO.Address.ZipCode
                };

                _context.Addresses.Add(address);
                _context.SaveChanges();

                addresses = _context.Addresses.Where(x => x.AddressId == address.AddressId).ToList();

                u.Address = addresses[0];
            }
        }
    }
}
