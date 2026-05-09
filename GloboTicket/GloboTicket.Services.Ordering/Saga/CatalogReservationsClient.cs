using System.Net.Http.Json;

namespace GloboTicket.Services.Ordering.Saga;

// Typed HttpClient for the catalog's reservation endpoints. Encapsulates
// the URL shape so the step handlers don't carry path strings.
//
// NOT idempotent against the catalog: a retry will decrement stock again.
// Acceptable for the demo; a production system would pass an idempotency
// key (e.g. orderId + eventId) and the catalog would deduplicate.
public interface ICatalogReservationsClient
{
    Task<bool> Reserve(Guid eventId, int count, CancellationToken ct = default);
    Task Release(Guid eventId, int count, CancellationToken ct = default);
}

public class CatalogReservationsClient : ICatalogReservationsClient
{
    private readonly HttpClient client;

    public CatalogReservationsClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<bool> Reserve(Guid eventId, int count, CancellationToken ct = default)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/events/{eventId}/reserve",
            new { count },
            ct);
        return response.IsSuccessStatusCode;
    }

    public async Task Release(Guid eventId, int count, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/events/{eventId}/reserve")
        {
            Content = JsonContent.Create(new { count })
        };
        var response = await client.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
