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

namespace EventManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly EventManagementContext _context;
        private readonly IEmailSender _emailSender;

        public TicketController(EventManagementContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Vraća sve ulaznice.
        /// </summary>
        // GET: api/Ticket
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
        {
            List<Ticket> tickets = await _context.Tickets.Include(t => t.TicketOwner).ToListAsync();
            return Ok(tickets);
        }

        /// <summary>
        /// Vraća specifične ulaznice po id-u događaja.
        /// </summary>
        // GET: api/Ticket/ByEvent/5
        [HttpGet("ByEvent/{id}")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets(int id)
        {
            List<Ticket> tickets = await _context.Tickets.Where(t => t.EventId == id).Include(t => t.TicketOwner).ToListAsync();
            return Ok(tickets);
        }

        /// <summary>
        /// Vraća specifičnu ulaznicu po id-u.
        /// </summary>
        // GET: api/Ticket/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            List<Ticket> tickets = await _context.Tickets.Include(t => t.TicketOwner).ToListAsync();
            Ticket t = tickets.FirstOrDefault(t => t.TicketId == id);

            if (t == null)
            {
                return NotFound();
            }

            return t;
        }

        /// <summary>
        /// Ažurira specifičnu ulaznicu po id-u.
        /// </summary>
        // PUT: api/Ticket/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return BadRequest();
            }

            var t = await _context.Tickets.FindAsync(id);
            if (t == null)
            {
                return NotFound();
            }

            t.TicketType = ticket.TicketType;
            t.Price = ticket.Price;
            t.QRCode = ticket.QRCode;
            t.PrintableToPdf = ticket.PrintableToPdf;
            t.EventId = ticket.EventId;
            t.Purchased = ticket.Purchased;

            GetTicketOwner(ticket, t);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!TicketExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Kupuje specifičnu ulaznicu po id-u za kontakta s e-mail adresom.
        /// </summary>
        // PUT: api/Ticket/Purchase/5/example@gmail.com
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Purchase/{id}/{ticketType}/{email}")]
        public async Task<IActionResult> UpdateTicketToPurchased(int id, string ticketType, string email)
        {
            if (!Enum.TryParse(ticketType, out TicketType tt))
            {
                return BadRequest("Invalid ticket type specified!");
            }

            List<Ticket> tickets = await _context.Tickets.ToListAsync();
            Ticket t = tickets.FirstOrDefault(t => t.Purchased == false && t.EventId == id && t.TicketType == (int)tt);

            if (t == null)
            {
                return BadRequest("No available tickets for specified event!");
            }

            List<Event> events = await _context.Events.ToListAsync();
            Event e = events.FirstOrDefault(e => e.EventId == id);

            /*var message = new Message(new string[] { email }, "Your ticket", $"You have purchased a ticket for \"{e.Title}\" starting at {e.StartDate}.");
            _emailSender.SendEmail(message);*/

            List<Contact> contacts = await _context.Contacts.ToListAsync();
            Contact c = contacts.FirstOrDefault(c => c.Email == email);
            if (c == null)
            {
                CreateContact(email, t);
            }
            else
            {
                t.ContactId = c.ContactId;
            }

            t.Purchased = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!TicketExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Objavljuje ulaznicu.
        /// </summary>
        // POST: api/Ticket
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Ticket>> CreateTicket(Ticket ticket)
        {
            var t = new Ticket
            {
                TicketType = ticket.TicketType,
                Price = ticket.Price,
                QRCode = ticket.QRCode,
                PrintableToPdf = ticket.PrintableToPdf,
                EventId = ticket.EventId,
                Purchased = ticket.Purchased
            };

            GetTicketOwner(ticket, t);

            _context.Tickets.Add(t);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTicket),
                new { id = t.TicketId },
                t);
        }

        /// <summary>
        /// Briše specifičnu ulaznicu po id-u.
        /// </summary>
        // DELETE: api/Ticket/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var t = await _context.Tickets.FindAsync(id);

            if (t == null)
            {
                return NotFound();
            }

            _context.Tickets.Remove(t);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TicketExists(int id)
        {
            return (_context.Tickets?.Any(t => t.TicketId == id)).GetValueOrDefault();
        }

        private void GetTicketOwner(Ticket ticket, Ticket t)
        {
            List<Contact> contacts = _context.Contacts.Where(x => x.ContactId == ticket.ContactId).ToList();
            if (contacts.Count > 0)
            {
                t.ContactId = contacts[0].ContactId;
                t.TicketOwner = contacts[0];
            }
        }

        private void CreateContact(string email, Ticket t)
        {
            var c = new Contact
            {
                FirstName = "FirstName",
                LastName = "LastName",
                Email = email
            };

            _context.Contacts.Add(c);
            _context.SaveChanges();

            t.ContactId = c.ContactId;
        }
    }
}
