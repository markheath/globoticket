using System;

namespace GloboTicket.Services.EventCatalog.Models
{
    public class TicketDto
    {
        public Guid TicketId { get; set; }
        public int Price { get; set; }
        public string Name { get; set; }
    }
}
