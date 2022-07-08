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
            ILookup<int, int> commentRatingsByEventId = comments.ToLookup(c => c.EventId, c => c.Rating);

            return Ok(comments.Average(c => c.Rating));
        }

        // GET: api/Statistics/TopRatedEvent
        [HttpGet("TopRatedEvent")]
        public async Task<ActionResult<KeyValuePair<double, EventDTO>>> GetTopRatedEvent()
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            ILookup<int, int> commentRatingsByEventId = comments.ToLookup(c => c.EventId, c => c.Rating);

            IOrderedEnumerable<IGrouping<int, int>> orderedCommentRatingsByEventId = commentRatingsByEventId.OrderByDescending(t => t.Average());

            IGrouping<int, int> grouping = orderedCommentRatingsByEventId.ElementAt(0);
            EventDTO eventDTO = EventToDTO(events.FirstOrDefault(e => e.EventId == grouping.Key));

            return Ok(new KeyValuePair<double, EventDTO>(grouping.Average(), eventDTO));
        }

        // GET: api/Statistics/MostCommentedEvent
        [HttpGet("MostCommentedEvent")]
        public async Task<ActionResult<KeyValuePair<int, EventDTO>>> GetMostCommentedEvent()
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            ILookup<int, Comment> commentsByEventId = comments.ToLookup(c => c.EventId);

            IOrderedEnumerable<IGrouping<int, Comment>> orderedCommentsByEventId = commentsByEventId.OrderByDescending(t => t.Count());

            IGrouping<int, Comment> grouping = orderedCommentsByEventId.ElementAt(0);
            EventDTO eventDTO = EventToDTO(events.FirstOrDefault(e => e.EventId == grouping.Key));

            return Ok(new KeyValuePair<int, EventDTO>(grouping.Count(), eventDTO));
        }

        // GET: api/Statistics/BestSellingEvent
        [HttpGet("BestSellingEvent")]
        public async Task<ActionResult<KeyValuePair<int, EventDTO>>> GetBestSellingEvent()
        {
            List<Ticket> tickets = await _context.Tickets.ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            ILookup<int, Ticket> ticketsByEventId = tickets.Where(t => t.Purchased == true).ToLookup(c => c.EventId);

            IOrderedEnumerable<IGrouping<int, Ticket>> orderedTicketsByEventId = ticketsByEventId.OrderByDescending(t => t.Count());

            IGrouping<int, Ticket> grouping = orderedTicketsByEventId.ElementAt(0);
            EventDTO eventDTO = EventToDTO(events.FirstOrDefault(e => e.EventId == grouping.Key));

            return Ok(new KeyValuePair<int, EventDTO>(grouping.Count(), eventDTO));
        }

        // GET: api/Statistics/BestSellingEvent/3
        [HttpGet("BestSellingEvent/{amount}")]
        public async Task<ActionResult<List<KeyValuePair<int, EventDTO>>>> GetBestSellingEvents(int amount)
        {
            if (amount > 3)
            {
                return BadRequest();
            }

            List<Ticket> tickets = await _context.Tickets.ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            ILookup<int, Ticket> ticketsByEventId = tickets.Where(t => t.Purchased == true).ToLookup(c => c.EventId);

            IOrderedEnumerable<IGrouping<int, Ticket>> orderedTicketsByEventId = ticketsByEventId.OrderByDescending(t => t.Count());
            List<KeyValuePair<int, EventDTO>> bestSellingEventsKeyValuePairs = new List<KeyValuePair<int, EventDTO>>();

            for (int i = 0; i < amount; i++)
            {
                IGrouping<int, Ticket> grouping = orderedTicketsByEventId.ElementAt(i);
                EventDTO eventDTO = EventToDTO(events.FirstOrDefault(e => e.EventId == grouping.Key));
                bestSellingEventsKeyValuePairs.Add((new KeyValuePair<int, EventDTO>(grouping.Count(), eventDTO)));
            }

            return Ok(bestSellingEventsKeyValuePairs);
        }

        [HttpGet("PercentageOfTicketsSoldByEventType")]
        public async Task<ActionResult<IDictionary<string, double>>> PercentageOfTicketsSoldByEventType()
        {
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).ToListAsync();
            List<Ticket> purchasedTickets = await _context.Tickets.Where(t => t.Purchased == true).ToListAsync();

            ILookup<int, string> eventTypesByEventIdLookup = events.ToLookup(e => e.EventId, e => Enum.GetName(typeof(EventType), e.EventType));
            IEnumerable<KeyValuePair<string, Ticket>> ticketsByEventType = purchasedTickets.Join(eventTypesByEventIdLookup,
                                 pt => pt.EventId,
                                 et => et.Key,
                                 (pt, et) => new KeyValuePair<string, Ticket>(et.FirstOrDefault(), pt));
            IOrderedEnumerable<KeyValuePair<string, Ticket>> orderedTicketsByEventType = ticketsByEventType.OrderBy(tbyt => tbyt.Key);

            Dictionary<string, double> purchasedTicketsByEventType = new Dictionary<string, double>();
            string selectedEventType = null;
            int count = 0;
            foreach (var item in orderedTicketsByEventType)
            {
                if (selectedEventType == null || selectedEventType != item.Key)
                {
                    selectedEventType = item.Key;

                    count = 0;
                    purchasedTicketsByEventType.Add(selectedEventType, 0);
                }

                purchasedTicketsByEventType[selectedEventType] += 1;
                count++;
            }

            purchasedTicketsByEventType.OrderBy(x => x.Value);

            double sum = purchasedTicketsByEventType.Values.Sum();
            foreach (var item in purchasedTicketsByEventType.Keys)
            {
                purchasedTicketsByEventType[item] /= sum;
            }

            return Ok(purchasedTicketsByEventType);
        }

        // GET: api/Statistics/UserWithMostEvents
        [HttpGet("UserWithMostEvents")]
        public async Task<ActionResult<KeyValuePair<int, UserDTO>>> GetUserWithMostEvents()
        {
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).Include(e => e.User).ToListAsync();
            List<User> users = await _context.Users.Include(u => u.Address).ToListAsync();
            ILookup<User, Event> eventsByUser = events.ToLookup(e => e.User);

            IOrderedEnumerable<IGrouping<User, Event>> orderedEventsByUser = eventsByUser.OrderByDescending(e => e.Count());

            IGrouping<User, Event> grouping = orderedEventsByUser.ElementAt(0);
            UserDTO userDTO = UserToDTO(users.FirstOrDefault(u => u.UserId == grouping.Key.UserId));

            return Ok(new KeyValuePair<int, UserDTO>(grouping.Count(), userDTO));
        }

        // GET: api/Statistics/UserWithMostSoldTickets
        [HttpGet("UserWithMostSoldTickets")]
        public async Task<ActionResult<KeyValuePair<int, UserDTO>>> GetUserWithMostSoldTickets()
        {
            List<Ticket> purchasedTickets = await _context.Tickets.Where(t => t.Purchased == true).ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.Location).ThenInclude(l => l.Address).Include(e => e.User).ToListAsync();
            List<User> users = await _context.Users.Include(u => u.Address).ToListAsync();
            ILookup<User, Event> eventsByUser = events.ToLookup(e => e.User);

            List<KeyValuePair<int, User>> usersBySoldTicketsTemp = new List<KeyValuePair<int, User>>();
            List<KeyValuePair<int, User>> usersBySoldTickets = new List<KeyValuePair<int, User>>();
            User selectedUser = null;
            int sum = 0;
            foreach (var item in eventsByUser)
            {
                if (selectedUser != null && selectedUser.UserId != item.Key.UserId)
                {
                    sum = usersBySoldTicketsTemp.Where(kvp => kvp.Value.UserId == selectedUser.UserId).Sum(kvp => kvp.Key);
                    usersBySoldTickets.Add(new KeyValuePair<int, User>(sum, selectedUser));
                }

                if (selectedUser == null || selectedUser.UserId != item.Key.UserId)
                {
                    selectedUser = users.FirstOrDefault(u => u.UserId == item.Key.UserId);
                }

                IEnumerable<Ticket> purchasedTicketsFromEvent = purchasedTickets.Join(item,
                                 t => t.EventId,
                                 e => e.EventId,
                                 (t, e) => t);

                usersBySoldTicketsTemp.Add(new KeyValuePair<int, User>(purchasedTicketsFromEvent.Count(), selectedUser));
            }

            sum = usersBySoldTicketsTemp.Where(kvp => kvp.Value.UserId == selectedUser.UserId).Sum(kvp => kvp.Key);
            usersBySoldTickets.Add(new KeyValuePair<int, User>(sum, selectedUser));

            KeyValuePair<int, User> keyValuePair = usersBySoldTickets.MaxBy(u => u.Key);

            return Ok(new KeyValuePair<int, UserDTO>(keyValuePair.Key, UserToDTO(keyValuePair.Value)));
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
            userDTO.UserType = user.UserType;

            return userDTO;
        }
    }
}
