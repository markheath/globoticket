namespace GloboTicket.Services.ShoppingBasket.Entities;

public class Event
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime Date { get; set; }
}
