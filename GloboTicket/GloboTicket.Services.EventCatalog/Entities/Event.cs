using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace GloboTicket.Services.EventCatalog.Entities
{
    public class Event
    {
        [Required]
        public Guid EventId { get; set; }
        public string Name { get; set; }
        // obsolete - to be removed - use Tickets collection instead
        public int Price { get; set; }
        public string Artist { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }
        public Collection<Ticket> Tickets { get; set; }
    }
}
