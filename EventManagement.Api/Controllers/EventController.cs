using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagement.Api.Models;
using EventManagement.Api.Models.DTO;
using EmailService;
using QRCoder;

namespace EventManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventManagementContext _context;
        private readonly IEmailSender _emailSender;

        public EventController(EventManagementContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: api/Event
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEvents()
        {
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            IEnumerable<EventDTO> eventDTOs = events.Select(x => EventToDTO(x));
            return Ok(eventDTOs);
        }

        // GET: api/Event/ByDateAll
        [HttpGet("ByDateAll")]
        public async Task<ActionResult<IEnumerable<EventBlockDTO>>> GetEventsByDateAll()
        {
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            List<EventBlockDTO> eventBlockDTOs = new List<EventBlockDTO>();
            List<DateTime> dateTimes = new List<DateTime>();
            foreach (var e in events)
            {
                if (!dateTimes.Any(dt => dt.Date == e.StartDate.Date))
                {
                    dateTimes.Add(e.StartDate);

                    eventBlockDTOs.Add(new EventBlockDTO
                    {
                        Date = e.StartDate.Date,
                        EventDTOs = new List<EventDTO>(events.Where(x => x.StartDate.Date == e.StartDate.Date).Select(x => EventToDTO(x)))
                    });
                }
            }
            return Ok(eventBlockDTOs);
        }

        // GET: api/Event/ByUser/5
        [HttpGet("ByUser/{id}")]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEventsByUser(int id)
        {
            List<Event> events = await _context.Events.Where(x => x.UserId == id).Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            IEnumerable<EventDTO> eventDTOs = events.Select(x => EventToDTO(x));
            return Ok(eventDTOs);
        }

        // GET: api/Event/ByDate/2001-01-01T09:09:17.490Z
        [HttpGet("ByDate/{date}")]
        public async Task<ActionResult<IEnumerable<EventBlockDTO>>> GetEventsByDate(string date)
        {
            DateTime.TryParse(date, out DateTime dateUtc);
            List<Event> events = await _context.Events.Where(x => x.StartDate.Date == dateUtc.Date).Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            EventBlockDTO eventBlockDTO = new EventBlockDTO
            {
                Date = dateUtc.Date,
                EventDTOs = new List<EventDTO>(events.Select(x => EventToDTO(x)))
            };
            return Ok(eventBlockDTO);
        }

        // GET: api/Event/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDTO>> GetEvent(int id)
        {
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            Event e = events.FirstOrDefault(e => e.EventId == id);

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

            if (!GetLocation(eventDTO, e))
            {
                return BadRequest("Location not found");
            }

            if (!GetUser(eventDTO, e))
            {
                return BadRequest("User not found");
            }

            CreateTickets(eventDTO, e);

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Event Updated!");
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

            if (!GetLocation(eventDTO, e))
            {
                return BadRequest("Location not found");
            }

            if (!GetUser(eventDTO, e))
            {
                return BadRequest("User not found");
            }

            _context.Events.Add(e);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }

            CreateTickets(eventDTO, e);

            await _context.SaveChangesAsync();

            return Ok("Event created!");
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

            List<Ticket> tickets = await _context.Tickets.Include(t => t.TicketOwner).ToListAsync();
            tickets.FirstOrDefault(t => t.EventId == e.EventId);

            List<string> emails = new List<string>();
            foreach (Ticket t in tickets)
            {
                if (t.TicketOwner != null)
                {
                    emails.Add(t.TicketOwner.Email);
                }
            }
            foreach (var email in emails)
            {
                var message = new Message(new string[] { email }, "Cancelled event", $"The event \"{e.Title}\" you purchased tickets for has been cancelled. Please contact support for refunds.");
                _emailSender.SendEmail(message);
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
            Location location = _context.Locations.Include(l => l.Address).Where(x => x.LocationId == e.LocationId).ToList().First();

            EventDTO eventDTO = new EventDTO();
            eventDTO.Id = e.EventId;
            eventDTO.Title = e.Title;
            eventDTO.StartDate = e.StartDate;
            eventDTO.EndDate = e.EndDate;
            eventDTO.Location = location;
            eventDTO.LocationId = location.LocationId;
            eventDTO.Username = users.First().Username;
            eventDTO.EventType = Enum.GetName(typeof(EventType), e.EventType);
            eventDTO.TicketsAvailable = tickets.Where(t => t.Purchased == false).Count();
            eventDTO.Picture = e.Picture;
            eventDTO.Comments = comments;
            return eventDTO;
        }

        private bool GetUser(EventDTO eventDTO, Event e)
        {
            List<User> users = _context.Users.Where(x => x.Username == eventDTO.Username).ToList();
            if (users.Count > 0)
            {
                e.UserId = users[0].UserId;
                e.User = users[0];
                return true;
            }
            return false;
        }

        private bool GetLocation(EventDTO eventDTO, Event e)
        {
            List<Location> locations = _context.Locations.Where(x => x.LocationId == eventDTO.LocationId).ToList();
            if (locations.Count > 0)
            {
                e.LocationId = locations[0].LocationId;
                e.Location = locations[0];
                return true;
            }
            return false;
        }

        private void CreateTickets(EventDTO eventDTO, Event e)
        {
            ICollection<Ticket> ticketsAvailable = new List<Ticket>();
            for (int i = 0; i < eventDTO.TicketsAvailable; i++)
            {
                ticketsAvailable.Add(new Ticket
                {
                    EventId = e.EventId,
                    Purchased = false,
                    PrintableToPdf = true
                });
            }
            _context.Tickets.AddRange(ticketsAvailable);
            _context.SaveChanges();

            foreach (var t in ticketsAvailable)
            {
                t.QRCode = GenerateQRCode(t.TicketId);
            }

            _context.SaveChanges();

            e.TicketsAvailable = ticketsAvailable;
        }

        private string GenerateQRCode(int id)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode($"{id}", QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string qrCodeImageAsBase64 = qrCode.GetGraphic(20);
            return qrCodeImageAsBase64;
        }
    }
}
