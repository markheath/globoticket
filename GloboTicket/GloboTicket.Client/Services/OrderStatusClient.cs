namespace GloboTicket.Web.Services;

// Pass-through to the ordering service's status endpoint. Same typed-
// client pattern as IEventCatalogService and IShoppingBasketService —
// MVC's default controller activator constructs controllers via
// ActivatorUtilities (NOT through DI), so AddHttpClient<TController>
// silently fails to wire the BaseAddress. Wrapping the HTTP call in a
// typed service that IS resolved through DI fixes that.
public interface IOrderStatusClient
{
    Task<HttpResponseMessage> GetStatus(Guid orderId);
}

public class OrderStatusClient(HttpClient client) : IOrderStatusClient
{
    public Task<HttpResponseMessage> GetStatus(Guid orderId) =>
        client.GetAsync($"/order/{orderId}/status");
}
