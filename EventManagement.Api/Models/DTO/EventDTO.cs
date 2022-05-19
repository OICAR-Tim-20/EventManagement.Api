using System;
using System.Collections.Generic;

namespace EventManagement.Api.Models.DTO
{
    public class EventDTO
    {
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Location Location { get; set; }
        public User User { get; set; }
        public int EventType { get; set; }
        public ICollection<Ticket> TicketsAvailable { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public string Picture { get; set; }
    }
}
