using System.Net.Http.Json;
using GloboTicket.Services.ShoppingBasket.Entities;

namespace GloboTicket.Services.ShoppingBasket.Services;

public class EventCatalogService(HttpClient client) : IEventCatalogService
{
    public async Task<Event> GetEvent(Guid id) =>
        await client.GetFromJsonAsync<Event>($"/api/events/{id}")
        ?? throw new InvalidOperationException($"Event {id} not found in catalog.");
}
