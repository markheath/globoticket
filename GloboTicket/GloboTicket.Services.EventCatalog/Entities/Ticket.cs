namespace GloboTicket.Services.EventCatalog.Entities;

public class Ticket
{
    public Guid TicketId { get; set; }
    public string Name { get; set; } = null!;
    public int Price { get; set; }

    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
}
