namespace GloboTicket.Services.Ordering.Entities;

// Owned value object inside an Order. Persisted into the parent's JSON
// column rather than its own table.
public class OrderLine
{
    public Guid EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public int Price { get; set; }
}
