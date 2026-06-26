using System.Net;
using GloboTicket.Web.Models;
using GloboTicket.Web.Models.Api;

namespace GloboTicket.Web.Services;

public class ShoppingBasketService(HttpClient client, Settings settings) : IShoppingBasketService
{
    public async Task<BasketLine> AddToBasket(Guid basketId, BasketLineForCreation basketLine)
    {
        if (basketId == Guid.Empty)
        {
            var basketResponse = await client.PostAsJsonAsync("/api/baskets", new BasketForCreation { UserId = settings.UserId });
            basketResponse.EnsureSuccessStatusCode();
            var basket = await basketResponse.Content.ReadFromJsonAsync<Basket>()
                ?? throw new InvalidOperationException("Basket creation returned null.");
            basketId = basket.BasketId;
        }

        var response = await client.PostAsJsonAsync($"api/baskets/{basketId}/basketlines", basketLine);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BasketLine>()
            ?? throw new InvalidOperationException("Basket line creation returned null.");
    }

    public async Task<Basket?> GetBasket(Guid basketId)
    {
        if (basketId == Guid.Empty)
            return null;
        var response = await client.GetAsync($"/api/baskets/{basketId}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Basket>();
    }

    public async Task<(bool ok, string? error, Basket? basket)> ApplyDiscountCode(Guid basketId, string code)
    {
        if (basketId == Guid.Empty)
            return (false, "Your basket is empty.", null);

        var response = await client.PutAsJsonAsync(
            $"/api/baskets/{basketId}/discount-code", new { code });

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            return (false, "That discount code is not valid.", null);
        }

        response.EnsureSuccessStatusCode();
        var basket = await response.Content.ReadFromJsonAsync<Basket>();
        return (true, null, basket);
    }

    public async Task RemoveDiscountCode(Guid basketId)
    {
        if (basketId == Guid.Empty)
            return;
        var response = await client.DeleteAsync($"/api/baskets/{basketId}/discount-code");
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<BasketLine>> GetLinesForBasket(Guid basketId)
    {
        if (basketId == Guid.Empty)
            return [];
        var response = await client.GetAsync($"/api/baskets/{basketId}/basketLines");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return [];
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BasketLine[]>() ?? [];
    }

    public async Task UpdateLine(Guid basketId, BasketLineForUpdate basketLineForUpdate)
    {
        var response = await client.PutAsJsonAsync(
            $"/api/baskets/{basketId}/basketLines/{basketLineForUpdate.LineId}", basketLineForUpdate);
        response.EnsureSuccessStatusCode();
    }

    public async Task RemoveLine(Guid basketId, Guid lineId)
    {
        var response = await client.DeleteAsync($"/api/baskets/{basketId}/basketLines/{lineId}");
        response.EnsureSuccessStatusCode();
    }
}
