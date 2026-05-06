using GloboTicket.Messages;
using MassTransit;

namespace GloboTicket.Services.Payment;

public class NewOrderHandler : IConsumer<PaymentRequestMessage>
{
    public Task Consume(ConsumeContext<PaymentRequestMessage> context)
    {
        Console.WriteLine($"Payment request received for basket id {context.Message.BasketId}.");
        return Task.CompletedTask;
    }
}

public class NewOrderHandlerV2 : IConsumer<PaymentRequestMessageV2>
{
    public Task Consume(ConsumeContext<PaymentRequestMessageV2> context)
    {
        Console.WriteLine($"Payment request received for order id {context.Message.OrderId}.");
        return Task.CompletedTask;
    }
}
