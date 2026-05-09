namespace GloboTicket.Services.Ordering.Saga;

// Step 2 — mocked card-charge step. Deterministic decline trigger: any
// PAN ending in "0000" is declined. This lets the demo show the saga's
// compensation path on demand without needing a real payment gateway.
//
// Successful charges produce an opaque txn_<guid> reference that the
// saga writes onto the Order row.
public class ChargeCardHandler
{
    public Task<object> Handle(
        ChargeCardRequested cmd,
        ILogger<ChargeCardHandler> logger,
        CancellationToken ct)
    {
        var pan = (cmd.CreditCardNumber ?? string.Empty).Trim();

        if (pan.EndsWith("0000"))
        {
            logger.LogWarning("Mock charge declined for order {OrderId} amount {Amount}",
                cmd.OrderId, cmd.Amount);
            return Task.FromResult<object>(
                new CardChargeFailed(cmd.OrderId, "Card declined"));
        }

        var paymentReference = $"txn_{Guid.NewGuid():N}";
        logger.LogInformation("Mock charge approved for order {OrderId} amount {Amount} as {Ref}",
            cmd.OrderId, cmd.Amount, paymentReference);
        return Task.FromResult<object>(
            new CardCharged(cmd.OrderId, paymentReference));
    }
}
