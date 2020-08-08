using System;

namespace GloboTicket.Messages
{
    public class PaymentRequestMessage
    {
        public Guid BasketId { get; set; }
    }

    public class PaymentRequestMessageV2
    {
        public Guid OrderId { get; set; }
    }
}
