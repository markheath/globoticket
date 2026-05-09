using GloboTicket.Services.EventCatalog.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Services.EventCatalog.Controllers;

// Reservations are commands rather than DTOs and aren't affected by the
// V1/V2 read-model split, so they live on their own un-versioned
// controller alongside the versioned EventController family.
[Route("api/events/{eventId}/reserve")]
[ApiController]
public class ReservationsController : ControllerBase
{
    private readonly IEventRepository _eventRepository;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(IEventRepository eventRepository, ILogger<ReservationsController> logger)
    {
        _eventRepository = eventRepository;
        _logger = logger;
    }

    public record ReservationRequest(int Count);

    // Reserve tickets for an event. Returns 409 when the event is sold
    // out or fewer tickets remain than were requested. Atomicity is
    // enforced in the repository's UPDATE ... WHERE TicketsAvailable >=
    // count predicate.
    [HttpPost]
    public async Task<IActionResult> Reserve(Guid eventId, [FromBody] ReservationRequest request)
    {
        var success = await _eventRepository.ReserveTickets(eventId, request.Count);
        if (!success)
        {
            _logger.LogInformation("Reservation refused for {EventId}: {Count} unavailable", eventId, request.Count);
            return Conflict(new { message = "Not enough tickets available" });
        }
        _logger.LogInformation("Reserved {Count} tickets for {EventId}", request.Count, eventId);
        return Ok();
    }

    // Release a previously made reservation. Called by the ordering
    // service to compensate when a later step in the order flow fails.
    [HttpDelete]
    public async Task<IActionResult> Release(Guid eventId, [FromBody] ReservationRequest request)
    {
        await _eventRepository.ReleaseTickets(eventId, request.Count);
        _logger.LogInformation("Released {Count} tickets for {EventId}", request.Count, eventId);
        return Ok();
    }
}
