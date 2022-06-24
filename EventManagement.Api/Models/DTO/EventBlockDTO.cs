using System;
using System.Collections.Generic;

namespace EventManagement.Api.Models.DTO
{
    public class EventBlockDTO
    {
        public DateTime Date { get; set; }
        public ICollection<EventDTO> EventDTOs { get; set; }
    }
}
