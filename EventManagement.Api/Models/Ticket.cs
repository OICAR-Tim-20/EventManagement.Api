using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.Api.Models
{
    public class Ticket
    {
        public Event Event { get; set; }
        public TicketType TicketType { get; set; }
        public double Price { get; set; }
        public string QRCode { get; set; }
        public Contact TicketOwner { get; set; }
        public bool PrintableToPdf { get; set; }

    }
}
