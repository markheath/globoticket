namespace GloboTicket.Services.EventCatalog.Models;

public class EventDto
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = null!;
    public int Price { get; set; }
    public string Artist { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
}
