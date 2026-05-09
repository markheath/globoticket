namespace GloboTicket.Services.Ordering.Saga;

// Compensation handler. Fires once per reservation the saga had
// previously made when a later step fails. Fire-and-forget; the saga
// has already moved on by the time the release runs.
public class ReleaseTicketsHandler
{
    public async Task Handle(
        ReleaseTicketsRequested cmd,
        ICatalogReservationsClient catalog,
        ILogger<ReleaseTicketsHandler> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Releasing {Count} tickets for {EventId} (order {OrderId})",
            cmd.Count, cmd.EventId, cmd.OrderId);
        await catalog.Release(cmd.EventId, cmd.Count, ct);
    }
}
