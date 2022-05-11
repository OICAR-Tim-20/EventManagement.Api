using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Api.Models
{
    public class Ticket
    {
        public int TicketId { get; set; }
        public Event Event { get; set; }
        public int TicketType { get; set; }
        public double Price { get; set; }
        public string QRCode { get; set; }
        [Required]
        public Contact TicketOwner { get; set; }
        public bool PrintableToPdf { get; set; } = false;
        public int EventId { get; set; }
        [Required]
        public int ContactId { get; set; }

    }
}
