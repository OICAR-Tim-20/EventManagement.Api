using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagement.Api.Models;
using EventManagement.Api.Models.StatisticsModel;

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
        public async Task<ActionResult<EventTypesByYear[]>> GetEventTypesByYear()
        {
            List<Event> events = await _context.Events.ToListAsync();

            ILookup<int, string> eventTypesByYearLookup = events.ToLookup(e => e.StartDate.Year, e => Enum.GetName(typeof(EventType), e.EventType));
            Dictionary<int, List<string>> eventTypesByYearDictionary = eventTypesByYearLookup.ToDictionary(x => x.Key, x => x.ToList());
            List<EventTypesByYear> eventTypesByYear = new List<EventTypesByYear>();

            foreach (var item in eventTypesByYearDictionary)
            {
                List<string> concerts = item.Value.FindAll(et => et == "Concert");
                List<string> festivals = item.Value.FindAll(et => et == "Festival");
                List<string> parties = item.Value.FindAll(et => et == "Party");

                eventTypesByYear.Add(new EventTypesByYear
                {
                    Year = item.Key,
                    NumberOfConcerts = concerts.Count,
                    NumberOfFestivals = festivals.Count,
                    NumberOfParties = parties.Count
                });
            }

            IOrderedEnumerable<EventTypesByYear> orderedEnumerable = eventTypesByYear.OrderByDescending(etby => etby.Year);

            return Ok(orderedEnumerable.ToArray());
        }

        // GET: api/Statistics/AverageRatingsByEvent
        [HttpGet("AverageRatingsByEvent")]
        public async Task<ActionResult<KeyValuePair<string, double>[]>> GetAverageRatingsByEvent()
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            List<Event> events = await _context.Events.ToListAsync();

            List<IGrouping<int, int>> commentRatingsByEventId = comments.ToLookup(c => c.EventId, c => c.Rating).ToList();
            List<KeyValuePair<string, double>> averageRatingsByEvent = new List<KeyValuePair<string, double>>();

            foreach (var e in events)
            {
                List<IGrouping<int, int>> list = commentRatingsByEventId.FindAll(crbei => crbei.Key == e.EventId);
                if (list.Count > 0)
                {
                    averageRatingsByEvent.Add(new KeyValuePair<string, double>(
                            $"{e.Title} ({e.StartDate.ToShortDateString()})", list[0].ToList().Average()));
                }
                else
                {
                    averageRatingsByEvent.Add(new KeyValuePair<string, double>(
                            $"{e.Title} ({e.StartDate.ToShortDateString()})", 0));
                }
            }

            return Ok(averageRatingsByEvent.ToArray());
        }

        // GET: api/Statistics/TopRatedEvents/5
        [HttpGet("TopRatedEvents/{amount}")]
        public async Task<ActionResult<KeyValuePair<string, double>[]>> GetTopRatedEvents(int amount)
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            List<Event> events = await _context.Events.ToListAsync();

            List<IGrouping<int, int>> commentRatingsByEventId = comments.ToLookup(c => c.EventId, c => c.Rating).ToList();
            List<KeyValuePair<string, double>> averageRatingsByEvent = new List<KeyValuePair<string, double>>();

            foreach (var e in events)
            {
                List<IGrouping<int, int>> list = commentRatingsByEventId.FindAll(crbei => crbei.Key == e.EventId);
                if (list.Count > 0)
                {
                    averageRatingsByEvent.Add(new KeyValuePair<string, double>(
                            $"{e.Title} ({e.StartDate.ToShortDateString()})", list[0].ToList().Average()));
                }
                else
                {
                    averageRatingsByEvent.Add(new KeyValuePair<string, double>(
                            $"{e.Title} ({e.StartDate.ToShortDateString()})", 0));
                }
            }

            IOrderedEnumerable<KeyValuePair<string, double>> orderedEnumerable = averageRatingsByEvent.OrderByDescending(arbe => arbe.Value);

            return Ok(orderedEnumerable.Take(amount).ToArray());
        }

        // GET: api/Statistics/MostCommentedEvents/5
        [HttpGet("MostCommentedEvents/{amount}")]
        public async Task<ActionResult<KeyValuePair<string, int>[]>> GetMostCommentedEvents(int amount)
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            List<Event> events = await _context.Events.ToListAsync();

            List<IGrouping<int, Comment>> commentsByEventId = comments.ToLookup(c => c.EventId).ToList();
            List<KeyValuePair<string, int>> totalCommentsByEvent = new List<KeyValuePair<string, int>>();

            foreach (var e in events)
            {
                List<IGrouping<int, Comment>> list = commentsByEventId.FindAll(cbei => cbei.Key == e.EventId);
                if (list.Count > 0)
                {
                    totalCommentsByEvent.Add(new KeyValuePair<string, int>(
                            $"{e.Title} ({e.StartDate.ToShortDateString()})", list[0].ToList().Count));
                }
                else
                {
                    totalCommentsByEvent.Add(new KeyValuePair<string, int>(
                            $"{e.Title} ({e.StartDate.ToShortDateString()})", 0));
                }
            }

            IOrderedEnumerable<KeyValuePair<string, int>> orderedEnumerable = totalCommentsByEvent.OrderByDescending(tcbe => tcbe.Value);

            return Ok(orderedEnumerable.Take(amount).ToArray());
        }

        // GET: api/Statistics/BestSellingEvents/5
        [HttpGet("BestSellingEvents/{amount}")]
        public async Task<ActionResult<KeyValuePair<string, int>[]>> GetBestSellingEvents(int amount)
        {
            List<Ticket> tickets = await _context.Tickets.ToListAsync();
            List<Event> events = await _context.Events.ToListAsync();

            List<IGrouping<int, Ticket>> ticketsByEventId = tickets.Where(t => t.Purchased == true).ToLookup(c => c.EventId).ToList();
            List<KeyValuePair<string, int>> totalTicketsSoldByEvent = new List<KeyValuePair<string, int>>();

            foreach (var e in events)
            {
                List<IGrouping<int, Ticket>> list = ticketsByEventId.FindAll(tbei => tbei.Key == e.EventId);
                if (list.Count > 0)
                {
                    totalTicketsSoldByEvent.Add(new KeyValuePair<string, int>(
                            $"{e.Title} ({e.StartDate.ToShortDateString()})", list[0].ToList().Count));
                }
                else
                {
                    totalTicketsSoldByEvent.Add(new KeyValuePair<string, int>(
                            $"{e.Title} ({e.StartDate.ToShortDateString()})", 0));
                }
            }

            IOrderedEnumerable<KeyValuePair<string, int>> orderedEnumerable = totalTicketsSoldByEvent.OrderByDescending(ttsbe => ttsbe.Value);

            return Ok(orderedEnumerable.Take(amount).ToArray());
        }

        [HttpGet("PercentageOfTicketsSoldByEventType")]
        public async Task<ActionResult<KeyValuePair<string, double>[]>> PercentageOfTicketsSoldByEventType()
        {
            List<Ticket> purchasedTickets = await _context.Tickets.Where(t => t.Purchased == true).ToListAsync();
            List<Event> events = await _context.Events.ToListAsync();

            ILookup<string, Event> eventsByEventTypeLookup = events.ToLookup(e => Enum.GetName(typeof(EventType), e.EventType));
            Dictionary<string, List<Event>> eventsByEventTypeDictionary = eventsByEventTypeLookup.ToDictionary(x => x.Key, x => x.ToList());
            List<KeyValuePair<string, int>> totalTicketsSoldByEventType = new List<KeyValuePair<string, int>>();

            foreach (var item in eventsByEventTypeDictionary)
            {
                IEnumerable<Ticket> purchasedTicketsFromEvent = purchasedTickets.Join(item.Value,
                                 t => t.EventId,
                                 e => e.EventId,
                                 (t, e) => t);

                if (purchasedTicketsFromEvent.ToList().Count > 0)
                {
                    totalTicketsSoldByEventType.Add(new KeyValuePair<string, int>(
                            item.Key, purchasedTicketsFromEvent.ToList().Count));
                }
                else
                {
                    totalTicketsSoldByEventType.Add(new KeyValuePair<string, int>(
                            item.Key, 0));
                }
            }

            double sum = 0;
            foreach (var item in totalTicketsSoldByEventType)
            {
                sum += item.Value;
            }
            List<KeyValuePair<string, double>> percentageSoldByEventType = new List<KeyValuePair<string, double>>();
            totalTicketsSoldByEventType.ForEach(tt => percentageSoldByEventType.Add(new KeyValuePair<string, double>(tt.Key, tt.Value / sum * 100)));

            return Ok(percentageSoldByEventType.ToArray());
        }

        // GET: api/Statistics/UsersWithMostEvents/5
        [HttpGet("UsersWithMostEvents/{amount}")]
        public async Task<ActionResult<KeyValuePair<string, int>[]>> GetUsersWithMostEvents(int amount)
        {
            List<User> users = await _context.Users.ToListAsync();
            List<Event> events = await _context.Events.ToListAsync();

            List<IGrouping<int, Event>> eventsByUserId = events.ToLookup(e => e.UserId).ToList();
            List<KeyValuePair<string, int>> totalEventsByUser = new List<KeyValuePair<string, int>>();

            foreach (var u in users)
            {
                List<IGrouping<int, Event>> list = eventsByUserId.FindAll(ebui => ebui.Key == u.UserId);
                if (list.Count > 0)
                {
                    totalEventsByUser.Add(new KeyValuePair<string, int>(
                            u.Username, list[0].ToList().Count));
                }
                else
                {
                    totalEventsByUser.Add(new KeyValuePair<string, int>(
                            u.Username, 0));
                }
            }

            IOrderedEnumerable<KeyValuePair<string, int>> orderedEnumerable = totalEventsByUser.OrderByDescending(tebu => tebu.Value);

            return Ok(orderedEnumerable.Take(amount).ToArray());
        }

        // GET: api/Statistics/UsersWithMostTicketsSold/5
        [HttpGet("UsersWithMostTicketsSold/{amount}")]
        public async Task<ActionResult<KeyValuePair<string, int>[]>> GetUsersWithMostTicketsSold(int amount)
        {
            List<Ticket> purchasedTickets = await _context.Tickets.Where(t => t.Purchased == true).ToListAsync();
            List<User> users = await _context.Users.Include(u => u.Address).ToListAsync();
            List<Event> events = await _context.Events.Include(e => e.User).ToListAsync();

            ILookup<User, Event> eventsByUserLookup = events.ToLookup(e => e.User);
            Dictionary<User, List<Event>> eventsByUserDictionary = eventsByUserLookup.ToDictionary(x => x.Key, x => x.ToList());
            List<KeyValuePair<string, int>> totalTicketsSoldByUser = new List<KeyValuePair<string, int>>();

            foreach (var item in eventsByUserDictionary)
            {
                IEnumerable<Ticket> purchasedTicketsFromEvent = purchasedTickets.Join(item.Value,
                                 t => t.EventId,
                                 e => e.EventId,
                                 (t, e) => t);

                if (purchasedTicketsFromEvent.ToList().Count > 0)
                {
                    totalTicketsSoldByUser.Add(new KeyValuePair<string, int>(
                            item.Key.Username, purchasedTicketsFromEvent.ToList().Count));
                }
                else
                {
                    totalTicketsSoldByUser.Add(new KeyValuePair<string, int>(
                            item.Key.Username, 0));
                }
            }

            IOrderedEnumerable<KeyValuePair<string, int>> orderedEnumerable = totalTicketsSoldByUser.OrderByDescending(ttsbu => ttsbu.Value);

            return Ok(orderedEnumerable.Take(amount).ToArray());
        }
    }
}
