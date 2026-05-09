namespace GloboTicket.Services.Ordering.Saga;

// Step 1 — call the catalog's reserve endpoint. Catalog returns 200 if
// the atomic decrement succeeded, 409 if the event was sold out or
// stock was insufficient. The handler converts that into a
// TicketsReserved or TicketsReservationFailed cascade so the saga can
// drive the next step.
public class ReserveTicketsHandler
{
    public async Task<object> Handle(
        ReserveTicketsRequested cmd,
        ICatalogReservationsClient catalog,
        ILogger<ReserveTicketsHandler> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Reserving {Count} tickets for {EventId} (order {OrderId})",
            cmd.Count, cmd.EventId, cmd.OrderId);

        var reserved = await catalog.Reserve(cmd.EventId, cmd.Count, ct);
        return reserved
            ? new TicketsReserved(cmd.OrderId, cmd.EventId, cmd.Count)
            : new TicketsReservationFailed(cmd.OrderId, cmd.EventId, cmd.Count,
                "Catalog refused reservation");
    }
}
