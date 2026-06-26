using GloboTicket.Services.ShoppingBasket.Mappings;
using GloboTicket.Services.ShoppingBasket.Models;
using GloboTicket.Services.ShoppingBasket.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Services.ShoppingBasket.Controllers;

[Route("api/baskets")]
[ApiController]
public class BasketsController : ControllerBase
{
    private readonly IBasketRepository _basketRepository;

    public BasketsController(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    [HttpGet("{basketId}", Name = "GetBasket")]
    public async Task<ActionResult<Basket>> Get(Guid basketId)
    {
        var basket = await _basketRepository.GetBasketById(basketId);
        if (basket == null)
            return NotFound();

        var discount = await ResolveDiscount(basket);
        return Ok(basket.ToModelWithSummary(discount));
    }

    // Applies a discount code to the basket. Stores only the code string;
    // the amount is recomputed from the current lines on every read.
    [HttpPut("{basketId}/discount-code")]
    public async Task<ActionResult<Basket>> ApplyDiscount(Guid basketId, [FromBody] ApplyDiscountCode request)
    {
        var basket = await _basketRepository.GetBasketById(basketId);
        if (basket == null)
            return NotFound();

        var discount = await _basketRepository.GetActiveDiscountCode(request.Code);
        if (discount == null)
            return BadRequest("That discount code is not valid.");

        basket.DiscountCode = discount.Code;
        await _basketRepository.SaveChanges();

        return Ok(basket.ToModelWithSummary(discount));
    }

    [HttpDelete("{basketId}/discount-code")]
    public async Task<ActionResult<Basket>> RemoveDiscount(Guid basketId)
    {
        var basket = await _basketRepository.GetBasketById(basketId);
        if (basket == null)
            return NotFound();

        basket.DiscountCode = null;
        await _basketRepository.SaveChanges();

        return Ok(basket.ToModelWithSummary(null));
    }

    // Resolves the basket's stored code to an active DiscountCode, or
    // null. A code that's been deactivated/removed since it was applied
    // simply drops off (no discount), which the next save will persist.
    private async Task<Entities.DiscountCode?> ResolveDiscount(Entities.Basket basket)
    {
        if (string.IsNullOrEmpty(basket.DiscountCode))
            return null;
        return await _basketRepository.GetActiveDiscountCode(basket.DiscountCode);
    }

    [HttpPost]
    public async Task<ActionResult<Basket>> Post(BasketForCreation basketForCreation)
    {
        var basketEntity = basketForCreation.ToEntity();

        _basketRepository.AddBasket(basketEntity);
        await _basketRepository.SaveChanges();

        return CreatedAtRoute(
            "GetBasket",
            new { basketId = basketEntity.BasketId },
            basketEntity.ToModel());
    }
}
