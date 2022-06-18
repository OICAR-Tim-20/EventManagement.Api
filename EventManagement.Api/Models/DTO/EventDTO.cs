using System;
using System.Collections.Generic;

namespace EventManagement.Api.Models.DTO
{
    public class EventDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Location Location { get; set; }
        public string Username { get; set; }
        public string EventType { get; set; }
        public int TicketsAvailable { get; set; }
        public string Picture { get; set; }
        public ICollection<Comment>? Comments { get; set; }
    }
}
