using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Api.Models
{
    public class Event
    {
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Location Location { get; set; }
        public User User { get; set; }
        public EventType EventType { get; set; }
        public List<Ticket> TicketsAvailable { get; set; }
        public List<Comment> Comments { get; set; }
        public string? Picture { get; set; }

    }
}
