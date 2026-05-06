namespace GloboTicket.Services.ShoppingBasket.Models;

public class Event
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime Date { get; set; }
}
