using GloboTicket.Messages;

namespace GloboTicket.Services.Ordering;

// LEGACY-COMPAT ARTIFACT — frozen in place on purpose.
//
// The live order path uses SubmitOrderCommand, handled by
// SubmitOrderHandler. These two handlers stay because
// PaymentRequestMessage / V2 are the teaching artifact for the
// message-versioning lesson: the demo shows that an upgraded service
// can still receive in-flight messages from a pre-upgrade publisher.
// Deleting them — or renaming the message types — defeats that lesson.
//
// The accompanying tests in GloboTicket.Services.Ordering.Tests are the
// contract guard: if either handler stops binding, the tests time out.
//
// The Wolverine notes below describe how that binding works.
//
// Wolverine handlers are plain classes with no marker interface. Discovery
// is by *naming convention*: at startup Wolverine scans the assembly for
// public types whose name ends in "Handler" or "Consumer", and binds public
// Handle / Consume methods on them to the message type of their first
// parameter.
//
// Watch the suffix carefully: "NewOrderHandlerV2" was *silently* skipped
// because it ends in "V2", not "Handler" — handler discovery doesn't error,
// it just doesn't bind, and at runtime PaymentRequestMessageV2 messages
// would land with no consumer. Hence the order is "NewOrderV2Handler",
// which still has "Handler" as its trailing word. (Alternatively, the
// [WolverineHandler] attribute opts a class in regardless of its name.)
//
// Logging is the same LoggerMessage source-generator pattern as before;
// nothing Wolverine-specific in how ILogger is injected.

public partial class NewOrderHandler(ILogger<NewOrderHandler> logger)
{
    public Task Handle(PaymentRequestMessage message)
    {
        LogPaymentReceived(logger, message.BasketId);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Payment request received for basket {BasketId}.")]
    static partial void LogPaymentReceived(ILogger logger, Guid basketId);
}

public partial class NewOrderV2Handler(ILogger<NewOrderV2Handler> logger)
{
    public Task Handle(PaymentRequestMessageV2 message)
    {
        LogPaymentReceived(logger, message.OrderId);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Payment request received for order {OrderId}.")]
    static partial void LogPaymentReceived(ILogger logger, Guid orderId);
}
