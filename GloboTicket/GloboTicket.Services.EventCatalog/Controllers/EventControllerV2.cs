using Asp.Versioning;
using GloboTicket.Services.EventCatalog.Mappings;
using GloboTicket.Services.EventCatalog.Models.V2;
using GloboTicket.Services.EventCatalog.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Services.EventCatalog.Controllers.V2;

[ApiVersion("2.0")]
[Route("api/events")]
[ApiController]
public class EventController : ControllerBase
{
    private readonly IEventRepository _eventRepository;

    public EventController(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> Get([FromQuery] Guid categoryId)
    {
        var events = await _eventRepository.GetEvents(categoryId);
        return Ok(events.Select(e => e.ToDtoV2()));
    }

    [HttpGet("{eventId}")]
    public async Task<ActionResult<EventDto>> GetById(Guid eventId)
    {
        var @event = await _eventRepository.GetEventById(eventId);
        if (@event is null)
            return NotFound();
        return Ok(@event.ToDtoV2());
    }
}
