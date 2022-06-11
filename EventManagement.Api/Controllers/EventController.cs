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
    public class EventController : ControllerBase
    {
        private readonly EventManagementContext _context;

        public EventController(EventManagementContext context)
        {
            _context = context;
        }

        // GET: api/Event
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEvents()
        {
            List<Event> events = await _context.Events.ToListAsync();
            events.ForEach(x => EventToDTO(x));
            return Ok(events);

            //memory leak
            /*return await _context.Events
                .Select(x => EventToDTO(x))
                .ToListAsync();*/
        }

        // GET: api/Event/ByUser/5
        [HttpGet("ByUser/{id}")]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEventsByUser(int id)
        {
            List<Event> events = await _context.Events.Where(x => x.UserId == id).ToListAsync();
            events.ForEach(x => EventToDTO(x));
            return Ok(events);

            //memory leak
            /*return await _context.Events
                .Where(x => x.UserId == id)
                .Select(x => EventToDTO(x))
                .ToListAsync();*/
        }

        // GET: api/Event/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDTO>> GetEvent(int id)
        {
            var e = await _context.Events.FindAsync(id);

            if (e == null)
            {
                return NotFound();
            }

            return EventToDTO(e);
        }

        // PUT: api/Event/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, EventDTO eventDTO)
        {
            if (id != eventDTO.Id)
            {
                return BadRequest();
            }

            var e = await _context.Events.FindAsync(id);
            if (e == null)
            {
                return NotFound();
            }

            e.Title = eventDTO.Title;
            e.StartDate = eventDTO.StartDate;
            e.EndDate = eventDTO.EndDate;
            e.Picture = eventDTO.Picture;

            if (Enum.TryParse(eventDTO.EventType, out EventType eventType))
            {
                e.EventType = (int)eventType;
            }

            GetOrCreateLocation(eventDTO, e);
            GetOrCreateUser(eventDTO, e);

            _context.SaveChanges();

            CreateTickets(eventDTO, e);
            CreateComments(eventDTO, e);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!EventExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Event
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EventDTO>> CreateEvent(EventDTO eventDTO)
        {
            var e = new Event
            {
                Title = eventDTO.Title,
                StartDate = eventDTO.StartDate,
                EndDate = eventDTO.EndDate,
                Picture = eventDTO.Picture
            };

            if (Enum.TryParse(eventDTO.EventType, out EventType eventType))
            {
                e.EventType = (int)eventType;
            }

            GetOrCreateLocation(eventDTO, e);
            GetOrCreateUser(eventDTO, e);

            _context.Events.Add(e);
            _context.SaveChanges();

            CreateTickets(eventDTO, e);
            CreateComments(eventDTO, e);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = e.EventId },
                EventToDTO(e));
        }

        // DELETE: api/Event/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var e = await _context.Events.FindAsync(id);

            if (e == null)
            {
                return NotFound();
            }

            _context.Events.Remove(e);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventExists(int id)
        {
            return (_context.Events?.Any(e => e.EventId == id)).GetValueOrDefault();
        }

        private EventDTO EventToDTO(Event e)
        {
            List<User> users = _context.Users.Where(x => x.UserId == e.UserId).ToList();
            List<Ticket> tickets = _context.Tickets.Where(x => x.EventId == e.EventId).ToList();
            List<Comment> comments = _context.Comments.Where(x => x.EventId == e.EventId).ToList();
            Location location = _context.Locations.Where(x => x.LocationId == e.LocationId).ToList().First();
            location.Address = _context.Addresses.Where(a => a.AddressId == location.AddressId).ToList().First();

            EventDTO eventDTO = new EventDTO();
            eventDTO.Id = e.EventId;
            eventDTO.Title = e.Title;
            eventDTO.StartDate = e.StartDate;
            eventDTO.EndDate = e.EndDate;
            eventDTO.Location = location;
            eventDTO.Username = users.First().Username;
            eventDTO.EventType = Enum.GetName(typeof(EventType), e.EventType);
            eventDTO.TicketsAvailable = tickets.Count;
            eventDTO.Picture = e.Picture;
            eventDTO.Comments = comments;
            return eventDTO;
            /*return new EventDTO
            {
                Id = e.EventId,
                Title = e.Title,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Location = e.Location,
                Username = users.First().Username,
                EventType = Enum.GetName(typeof(EventType), e.EventType),
                TicketsAvailable = e.TicketsAvailable.Count,
                Picture = e.Picture,
                Comments = e.Comments
            };*/
        }


        private void GetOrCreateUser(EventDTO eventDTO, Event e)
        {
            //Treba se prebacit na user controller
            List<User> users = _context.Users.Where(x => x.Username == eventDTO.Username).ToList();
            if (users.Count > 0)
            {
                e.UserId = users[0].UserId;
                e.User = users[0];
            }
            else
            {
                //placeholder podaci
                User user = new User
                {
                    Username = eventDTO.Username,
                    Email = "example@mail.com",
                    PasswordSalt = Convert.FromBase64String("CGYzqeN4plZekNC88Umm1Q=="),
                    PasswordHash = Convert.FromBase64String("Gt9Yc4AiIvmsC1QQbe2RZsCIqvoYlst2xbz0Fs8aHnw=")
                };
                _context.Users.Add(user);
                users = _context.Users.Where(x => x.Username == eventDTO.Username).ToList();

                e.UserId = users[0].UserId;
                e.User = users[0];
            }
        }

        private void GetOrCreateLocation(EventDTO eventDTO, Event e)
        {
            List<int> locationIdList = _context.Locations.Select(x => x.LocationId).Where(y => y == eventDTO.Location.LocationId).ToList();
            if (locationIdList.Count > 0)
            {
                e.LocationId = eventDTO.Location.LocationId;
                e.Location = eventDTO.Location;
            }
            else
            {
                List<Address> addresses = _context.Addresses.Where(x => x.AddressId == eventDTO.Location.AddressId).ToList();
                if (addresses.Count == 0)
                {
                    Address address = new Address
                    {
                        City = eventDTO.Location.Address.City,
                        HouseNumber = eventDTO.Location.Address.HouseNumber,
                        Street = eventDTO.Location.Address.Street,
                        ZipCode = eventDTO.Location.Address.ZipCode
                    };
                    _context.Addresses.Add(address);
                    _context.SaveChanges();

                    addresses = _context.Addresses.Where(x => x.AddressId == address.AddressId).ToList();
                }

                Location location = new Location
                {
                    LocationId = eventDTO.Location.LocationId,
                    AddressId = addresses[0].AddressId,
                    Address = addresses[0],
                    Venue = eventDTO.Location.Venue
                };
                _context.Locations.Add(location);
                _context.SaveChanges();

                List<Location> locations = _context.Locations.Where(x => x.LocationId == location.LocationId).ToList();

                e.LocationId = locations[0].LocationId;
                e.Location = locations[0];
            }
        }

        private void CreateTickets(EventDTO eventDTO, Event e)
        {
            ICollection<Ticket> ticketsAvailable = new List<Ticket>();
            for (int i = 0; i < eventDTO.TicketsAvailable; i++)
            {
                ticketsAvailable.Add(new Ticket());
            }
            ticketsAvailable.ToList().ForEach(x => x.EventId = e.EventId);
            ticketsAvailable.ToList().ForEach(x => _context.Tickets.Add(x));
            e.TicketsAvailable = ticketsAvailable;
        }

        private void CreateComments(EventDTO eventDTO, Event e)
        {
            eventDTO.Comments.ToList().ForEach(x => x.EventId = e.EventId);
            eventDTO.Comments.ToList().ForEach(x => _context.Comments.Add(x));
            e.Comments = eventDTO.Comments.ToList();
        }
    }
}
