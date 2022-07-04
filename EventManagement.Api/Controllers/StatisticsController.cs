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
    public class StatisticsController : ControllerBase
    {
        private readonly EventManagementContext _context;

        public StatisticsController(EventManagementContext context)
        {
            _context = context;
        }

        // GET: api/Statistics/EventTypesByYear
        [HttpGet("EventTypesByYear")]
        public async Task<ActionResult<IDictionary<int, List<KeyValuePair<string, int>>>>> GetEventTypesByYear()
        {
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).OrderBy(e => e.StartDate.Year).ThenBy(e => e.EventType).ToListAsync();
            ILookup<int, string> eventTypesByYearLookup = events.ToLookup(e => e.StartDate.Year, e => Enum.GetName(typeof(EventType), e.EventType));
            Dictionary<int, List<string>> eventTypesByYearDictionary = eventTypesByYearLookup.ToDictionary(x => x.Key, x => x.ToList());

            Dictionary<int, List<KeyValuePair<string, int>>> eventTypesByYearFinal = new Dictionary<int, List<KeyValuePair<string, int>>>();
            string selectedEventType = "";
            int eventTypeCount = 0;
            foreach (var item in eventTypesByYearDictionary)
            {
                List<KeyValuePair<string, int>> keyValuePairs = new List<KeyValuePair<string, int>>();

                foreach (var eventType in item.Value)
                {

                    if (eventTypeCount != 0 && selectedEventType != eventType)
                    {
                        keyValuePairs.Add(new KeyValuePair<string, int>(selectedEventType, eventTypeCount));
                    }

                    if (selectedEventType != eventType)
                    {
                        eventTypeCount = 0;
                        selectedEventType = eventType;
                    }

                    eventTypeCount += 1;
                }

                keyValuePairs.Add(new KeyValuePair<string, int>(selectedEventType, eventTypeCount));
                eventTypesByYearFinal.Add(item.Key, keyValuePairs);
                selectedEventType = "";
                eventTypeCount = 0;
            }
            return Ok(eventTypesByYearFinal);
        }

        // GET: api/Statistics/AverageEventRating
        [HttpGet("AverageEventRating")]
        public async Task<ActionResult<double>> GetAverageEventRating()
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            ILookup<int, int> commentRatingsByEventId = comments.ToLookup(c => c.EventId, c => c.Rating);

            Event selectedEvent = null;
            List<int> totalEventRatings = new List<int>();
            foreach (var item in commentRatingsByEventId)
            {
                if (selectedEvent == null || selectedEvent.EventId != item.Key)
                {
                    selectedEvent = events.FirstOrDefault(e => e.EventId == item.Key);
                }
                totalEventRatings.AddRange(item);
            }
            return Ok(totalEventRatings.Average());
        }

        // GET: api/Statistics/TopRatedEvent
        [HttpGet("TopRatedEvent")]
        public async Task<ActionResult<KeyValuePair<double, EventDTO>>> GetTopRatedEvent()
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            ILookup<int, int> commentRatingsByEventId = comments.ToLookup(c => c.EventId, c => c.Rating);

            Event selectedEvent = null;
            Event topRatedEvent = null;
            double maxRating = 0;
            foreach (var item in commentRatingsByEventId)
            {
                if (selectedEvent == null || selectedEvent.EventId != item.Key)
                {
                    selectedEvent = events.FirstOrDefault(e => e.EventId == item.Key);
                }

                if (maxRating < item.Average())
                {
                    maxRating = item.Average();
                    topRatedEvent = selectedEvent;
                }
            }
            return Ok(new KeyValuePair<double, EventDTO>(maxRating, EventToDTO(topRatedEvent)));
        }

        // GET: api/Statistics/MostCommentedEvent
        [HttpGet("MostCommentedEvent")]
        public async Task<ActionResult<KeyValuePair<int, EventDTO>>> GetMostCommentedEvent()
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            ILookup<int, Comment> commentsByEventId = comments.ToLookup(c => c.EventId);

            Event selectedEvent = null;
            Event mostCommentedEvent = null;
            int mostComments = 0;
            foreach (var item in commentsByEventId)
            {
                if (selectedEvent == null || selectedEvent.EventId != item.Key)
                {
                    selectedEvent = events.FirstOrDefault(e => e.EventId == item.Key);
                }

                if (mostComments < item.Count())
                {
                    mostComments = item.Count();
                    mostCommentedEvent = selectedEvent;
                }
            }
            return Ok(new KeyValuePair<int, EventDTO>(mostComments, EventToDTO(mostCommentedEvent)));
        }

        // GET: api/Statistics/BestSellingEvent
        [HttpGet("BestSellingEvent")]
        public async Task<ActionResult<KeyValuePair<int, EventDTO>>> GetBestSellingEvent()
        {
            List<Ticket> tickets = await _context.Tickets.ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            ILookup<int, Ticket> ticketsByEventId = tickets.Where(t => t.Purchased == true).ToLookup(c => c.EventId);

            Event selectedEvent = null;
            Event bestSellingEvent = null;
            int mostSoldTickets = 0;
            foreach (var item in ticketsByEventId)
            {
                if (selectedEvent == null || selectedEvent.EventId != item.Key)
                {
                    selectedEvent = events.FirstOrDefault(e => e.EventId == item.Key);
                }

                if (mostSoldTickets < item.Count())
                {
                    mostSoldTickets = item.Count();
                    bestSellingEvent = selectedEvent;
                }
            }
            return Ok(new KeyValuePair<int, EventDTO>(mostSoldTickets, EventToDTO(bestSellingEvent)));
        }

        // GET: api/Statistics/UserWithMostEvents
        [HttpGet("UserWithMostEvents")]
        public async Task<ActionResult<KeyValuePair<int, UserDTO>>> GetUserWithMostEvents()
        {
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).Include(e => e.User).ToListAsync();
            List<User> users = await _context.Users.Include(u => u.Address).ToListAsync();
            ILookup<User, Event> eventsByUser = events.ToLookup(e => e.User);

            User selectedUser = null;
            User userWithMostEvents = null;
            int mostEvents = 0;
            foreach (var item in eventsByUser)
            {
                if (selectedUser == null || selectedUser.UserId != item.Key.UserId)
                {
                    selectedUser = users.FirstOrDefault(u => u.UserId == item.Key.UserId);
                }

                if (mostEvents < item.Count())
                {
                    mostEvents = item.Count();
                    userWithMostEvents = selectedUser;
                }
            }
            return Ok(new KeyValuePair<int, UserDTO>(mostEvents, UserToDTO(userWithMostEvents)));
        }

        // GET: api/Statistics/UserWithMostSoldTickets
        [HttpGet("UserWithMostSoldTickets")]
        public async Task<ActionResult<KeyValuePair<int, UserDTO>>> GetUserWithMostSoldTickets()
        {
            List<Ticket> purchasedTickets = await _context.Tickets.Where(t => t.Purchased == true).ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).Include(e => e.User).ToListAsync();
            List<User> users = await _context.Users.Include(u => u.Address).ToListAsync();
            ILookup<User, Event> eventsByUser = events.ToLookup(e => e.User);

            User selectedUser = null;
            User userWithMostTickets = null;
            int totalTickets = 0;
            int mostSoldTickets = 0;
            foreach (var item in eventsByUser)
            {
                if (selectedUser == null || selectedUser.UserId != item.Key.UserId)
                {
                    totalTickets = 0;
                    selectedUser = users.FirstOrDefault(u => u.UserId == item.Key.UserId);
                }

                IEnumerable<Ticket> purchasedTicketsFromEvent = purchasedTickets.Join(item,
                                 t => t.EventId,
                                 e => e.EventId,
                                 (t, e) => t);

                totalTickets += purchasedTicketsFromEvent.Count();

                if (mostSoldTickets < totalTickets)
                {
                    mostSoldTickets = totalTickets;
                    userWithMostTickets = selectedUser;
                }
            }
            return Ok(new KeyValuePair<int, UserDTO>(mostSoldTickets, UserToDTO(userWithMostTickets)));
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
            eventDTO.TicketsAvailable = tickets.Count;
            eventDTO.Picture = e.Picture;
            eventDTO.Comments = comments;
            return eventDTO;
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
    }
}
