using GloboTicket.Services.EventCatalog.Entities;

namespace GloboTicket.Services.EventCatalog.Repositories;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetEvents(Guid categoryId);
    Task<Event?> GetEventById(Guid eventId);
    Task<bool> ReserveTickets(Guid eventId, int count);
    Task ReleaseTickets(Guid eventId, int count);
}
