using GloboTicket.Services.EventCatalog.Entities;
using GloboTicket.Services.EventCatalog.Models;
using V2 = GloboTicket.Services.EventCatalog.Models.V2;

namespace GloboTicket.Services.EventCatalog.Mappings;

public static class EventCatalogMappings
{
    public static EventDto ToDto(this Event @event) => new()
    {
        EventId = @event.EventId,
        Name = @event.Name,
        // V1 surfaced a single Price; v2 replaced it with the full Tickets collection.
        Price = @event.Tickets.Min(t => t.Price),
        Artist = @event.Artist,
        Date = @event.Date,
        Description = @event.Description,
        ImageUrl = @event.ImageUrl,
        CategoryId = @event.CategoryId,
        CategoryName = @event.Category.Name,
        TicketsAvailable = @event.TicketsAvailable,
    };

    public static V2.EventDto ToDtoV2(this Event @event) => new()
    {
        EventId = @event.EventId,
        Name = @event.Name,
        Artist = @event.Artist,
        Date = @event.Date,
        Description = @event.Description,
        ImageUrl = @event.ImageUrl,
        CategoryId = @event.CategoryId,
        CategoryName = @event.Category.Name,
        Tickets = [.. @event.Tickets.Select(t => t.ToDto())],
        TicketsAvailable = @event.TicketsAvailable,
    };

    public static CategoryDto ToDto(this Category category) => new()
    {
        CategoryId = category.CategoryId,
        Name = category.Name,
    };

    public static TicketDto ToDto(this Ticket ticket) => new()
    {
        TicketId = ticket.TicketId,
        Name = ticket.Name,
        Price = ticket.Price,
    };
}
