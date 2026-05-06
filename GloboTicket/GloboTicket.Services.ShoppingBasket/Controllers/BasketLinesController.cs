using GloboTicket.Services.ShoppingBasket.Mappings;
using GloboTicket.Services.ShoppingBasket.Models;
using GloboTicket.Services.ShoppingBasket.Repositories;
using GloboTicket.Services.ShoppingBasket.Services;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Services.ShoppingBasket.Controllers;

[Route("api/baskets/{basketId}/basketlines")]
[ApiController]
public class BasketLinesController : ControllerBase
{
    private readonly IBasketRepository _basketRepository;
    private readonly IBasketLinesRepository _basketLinesRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IEventCatalogService _eventCatalogService;

    public BasketLinesController(
        IBasketRepository basketRepository,
        IBasketLinesRepository basketLinesRepository,
        IEventRepository eventRepository,
        IEventCatalogService eventCatalogService)
    {
        _basketRepository = basketRepository;
        _basketLinesRepository = basketLinesRepository;
        _eventRepository = eventRepository;
        _eventCatalogService = eventCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BasketLine>>> Get(Guid basketId)
    {
        if (!await _basketRepository.BasketExists(basketId))
            return NotFound();

        var basketLines = await _basketLinesRepository.GetBasketLines(basketId);
        return Ok(basketLines.Select(bl => bl.ToModel()));
    }

    [HttpGet("{basketLineId}", Name = "GetBasketLine")]
    public async Task<ActionResult<BasketLine>> Get(Guid basketId, Guid basketLineId)
    {
        if (!await _basketRepository.BasketExists(basketId))
            return NotFound();

        var basketLine = await _basketLinesRepository.GetBasketLineById(basketLineId);
        if (basketLine == null)
            return NotFound();

        return Ok(basketLine.ToModel());
    }

    [HttpPost]
    public async Task<ActionResult<BasketLine>> Post(Guid basketId, [FromBody] BasketLineForCreation basketLineForCreation)
    {
        if (!await _basketRepository.BasketExists(basketId))
            return NotFound();

        if (!await _eventRepository.EventExists(basketLineForCreation.EventId))
        {
            var eventFromCatalog = await _eventCatalogService.GetEvent(basketLineForCreation.EventId);
            _eventRepository.AddEvent(eventFromCatalog);
            await _eventRepository.SaveChanges();
        }

        var basketLineEntity = basketLineForCreation.ToEntity();

        var processedBasketLine = await _basketLinesRepository.AddOrUpdateBasketLine(basketId, basketLineEntity);
        await _basketLinesRepository.SaveChanges();

        return CreatedAtRoute(
            "GetBasketLine",
            new { basketId = basketLineEntity.BasketId, basketLineId = basketLineEntity.BasketLineId },
            processedBasketLine.ToModel());
    }

    [HttpPut("{basketLineId}")]
    public async Task<ActionResult<BasketLine>> Put(Guid basketId, Guid basketLineId, [FromBody] BasketLineForUpdate basketLineForUpdate)
    {
        if (!await _basketRepository.BasketExists(basketId))
            return NotFound();

        var basketLineEntity = await _basketLinesRepository.GetBasketLineById(basketLineId);
        if (basketLineEntity == null)
            return NotFound();

        basketLineForUpdate.ApplyTo(basketLineEntity);

        _basketLinesRepository.UpdateBasketLine(basketLineEntity);
        await _basketLinesRepository.SaveChanges();

        return Ok(basketLineEntity.ToModel());
    }

    [HttpDelete("{basketLineId}")]
    public async Task<IActionResult> Delete(Guid basketId, Guid basketLineId)
    {
        if (!await _basketRepository.BasketExists(basketId))
            return NotFound();

        var basketLineEntity = await _basketLinesRepository.GetBasketLineById(basketLineId);
        if (basketLineEntity == null)
            return NotFound();

        _basketLinesRepository.RemoveBasketLine(basketLineEntity);
        await _basketLinesRepository.SaveChanges();

        return NoContent();
    }
}
