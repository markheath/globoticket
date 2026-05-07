using GloboTicket.Messages;
using MassTransit;

namespace GloboTicket.Services.Payment;

public partial class NewOrderHandler(ILogger<NewOrderHandler> logger) : IConsumer<PaymentRequestMessage>
{
    public Task Consume(ConsumeContext<PaymentRequestMessage> context)
    {
        LogPaymentReceived(logger, context.Message.BasketId);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Payment request received for basket {BasketId}.")]
    static partial void LogPaymentReceived(ILogger logger, Guid basketId);
}

public partial class NewOrderHandlerV2(ILogger<NewOrderHandlerV2> logger) : IConsumer<PaymentRequestMessageV2>
{
    public Task Consume(ConsumeContext<PaymentRequestMessageV2> context)
    {
        LogPaymentReceived(logger, context.Message.OrderId);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Payment request received for order {OrderId}.")]
    static partial void LogPaymentReceived(ILogger logger, Guid orderId);
}
