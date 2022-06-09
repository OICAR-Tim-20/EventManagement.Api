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
            return await _context.Events
                .Select(x => EventToDTO(x))
                .ToListAsync();
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

            //Treba modificirati Event
            e.Title = eventDTO.Title;
            //...

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
            //Treba napraviti Event
            var e = new Event();
            //...

            _context.Events.Add(e);
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

        private static EventDTO EventToDTO(Event e) =>
            //Treba izvuci ostale properties iz Eventa
            new EventDTO
            {
                Id = e.EventId,
                Title = e.Title,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                EventType = Enum.GetName(typeof(EventType), e.EventType),
                Picture = e.Picture
            };
            //...
    }
}
