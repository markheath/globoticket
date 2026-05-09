using System.Net;
using System.Net.Http.Json;

namespace GloboTicket.Web.Services;

// Pass-through to the ordering service's order endpoint. Same typed-
// client pattern as IEventCatalogService and IShoppingBasketService —
// MVC's default controller activator constructs controllers via
// ActivatorUtilities (NOT through DI), so AddHttpClient<TController>
// silently fails to wire the BaseAddress. Wrapping the HTTP call in a
// typed service that IS resolved through DI fixes that.
public interface IOrderStatusClient
{
    Task<OrderStatusResult?> GetStatus(Guid orderId);
}

public record OrderStatusResult(string Status, string? FailureReason);

public class OrderStatusClient(HttpClient client) : IOrderStatusClient
{
    public async Task<OrderStatusResult?> GetStatus(Guid orderId)
    {
        var response = await client.GetAsync($"/order/{orderId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderStatusResult>();
    }
}
