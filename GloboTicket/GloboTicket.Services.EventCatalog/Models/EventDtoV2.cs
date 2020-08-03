using System;
using System.Collections.ObjectModel;

namespace GloboTicket.Services.EventCatalog.Models
{
    public class EventDtoV2
    {
        public Guid EventId { get; set; }
        public string Name { get; set; }
        public string Artist { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Collection<TicketDto> Tickets { get; set; }
    }
}
