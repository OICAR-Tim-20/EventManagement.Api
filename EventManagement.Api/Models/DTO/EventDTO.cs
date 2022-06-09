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
        public string EventType { get; set; }
        public int TicketsAvailable { get; set; }

        public string Username { get; set; }

        public string Picture { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string Venue { get; set; }
    }
}
