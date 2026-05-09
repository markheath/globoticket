using System.Collections.ObjectModel;

namespace GloboTicket.Services.EventCatalog.Models.V2;

public class EventDto
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = null!;
    public string Artist { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public Collection<TicketDto> Tickets { get; set; } = [];
    public int TicketsAvailable { get; set; }
}
