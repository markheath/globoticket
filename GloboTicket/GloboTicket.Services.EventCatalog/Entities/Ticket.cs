using System;

namespace GloboTicket.Services.EventCatalog.Entities
{
    public class Ticket
    {
        public Guid TicketId { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }

        // the event this ticket relates to
        public Guid EventId { get; set; }
        public Event Event { get; set; }
    }
}
