using GloboTicket.Services.EventCatalog.DbContexts;
using GloboTicket.Services.EventCatalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.EventCatalog.Repositories
{
    public class EventRepository: IEventRepository
    {
        private readonly EventCatalogDbContext _eventCatalogDbContext;

        public EventRepository(EventCatalogDbContext eventCatalogDbContext)
        {
            _eventCatalogDbContext = eventCatalogDbContext;
        }


        public async Task<IEnumerable<Event>> GetEvents(Guid categoryId)
        {
            return await _eventCatalogDbContext.Events
                .Include(x => x.Category)
                .Include(x => x.Tickets)
                .Where(x => (x.CategoryId == categoryId || categoryId == Guid.Empty)).ToListAsync();
        }

        public async Task<Event?> GetEventById(Guid eventId)
        {
            return await _eventCatalogDbContext.Events
                .Include(x => x.Category)
                .Include(x => x.Tickets)
                .Where(x => x.EventId == eventId)
                .FirstOrDefaultAsync();
        }

        // Atomic decrement at the database level: the row is only updated
        // if enough stock exists. Returns false if the event is sold out
        // or the requested quantity exceeds availability. The atomicity
        // matters because two concurrent reservations for the last
        // ticket must not both succeed.
        public async Task<bool> ReserveTickets(Guid eventId, int count)
        {
            var rowsAffected = await _eventCatalogDbContext.Events
                .Where(e => e.EventId == eventId && e.TicketsAvailable >= count)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    e => e.TicketsAvailable,
                    e => e.TicketsAvailable - count));
            return rowsAffected > 0;
        }

        // Returns previously reserved stock. Called by the ordering
        // service to compensate when a later step in the order flow
        // fails (e.g. card declined after stock was reserved).
        public async Task ReleaseTickets(Guid eventId, int count)
        {
            await _eventCatalogDbContext.Events
                .Where(e => e.EventId == eventId)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    e => e.TicketsAvailable,
                    e => e.TicketsAvailable + count));
        }
    }
}
