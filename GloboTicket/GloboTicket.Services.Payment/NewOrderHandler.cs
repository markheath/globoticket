using System;
using System.Threading.Tasks;
using GloboTicket.Messages;
using Rebus.Handlers;

namespace GloboTicket.Services.Payment
{
    public class NewOrderHandler : IHandleMessages<PaymentRequestMessage>
    {
        public Task Handle(PaymentRequestMessage message)
        {
            Console.WriteLine($"Payment request received for basket id {message.BasketId}.");
            return Task.CompletedTask;
        }
    }

    public class NewOrderHandlerV2 : IHandleMessages<PaymentRequestMessageV2>
    {
        public Task Handle(PaymentRequestMessageV2 message)
        {
            Console.WriteLine($"Payment request received for order id {message.OrderId}.");
            return Task.CompletedTask;
        }
    }
}
