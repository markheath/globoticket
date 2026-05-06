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

        var result = basket.ToModel();
        result.NumberOfItems = basket.BasketLines.Sum(bl => bl.TicketAmount);
        return Ok(result);
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
