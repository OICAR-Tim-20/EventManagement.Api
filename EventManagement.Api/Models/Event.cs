using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Api.Models
{
    public class Event
    {
        public int EventId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime? EndDate { get; set; }
        [Required]
        public Location Location { get; set; }
        [Required]
        public int LocationId { get; set; }
        [Required]
        public User User { get; set; }
        public int UserId {get; set;}
        public int EventType { get; set; }
        public ICollection<Ticket> TicketsAvailable { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public string Picture { get; set; }
    }
}
