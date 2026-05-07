using GloboTicket.Web.Models.Api;

namespace GloboTicket.Web.Services;

public class EventCatalogService(HttpClient client) : IEventCatalogService
{
    public async Task<IEnumerable<Event>> GetAll() =>
        await client.GetFromJsonAsync<List<Event>>("/api/events") ?? [];

    public async Task<IEnumerable<Event>> GetByCategoryId(Guid categoryId) =>
        await client.GetFromJsonAsync<List<Event>>($"/api/events?categoryId={categoryId}") ?? [];

    public async Task<Event> GetEvent(Guid id) =>
        await client.GetFromJsonAsync<Event>($"/api/events/{id}")
        ?? throw new InvalidOperationException($"Event {id} not found.");

    public async Task<IEnumerable<Category>> GetCategories() =>
        await client.GetFromJsonAsync<List<Category>>("/api/categories") ?? [];
}
