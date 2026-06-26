namespace GloboTicket.Web.Models.Api
{
    public class Basket
    {
        public Guid BasketId { get; set; }
        public Guid UserId { get; set; }
        public int NumberOfItems { get; set; }
        public int Subtotal { get; set; }
        public string? DiscountCode { get; set; }
        public int DiscountAmount { get; set; }
        public int Total { get; set; }
    }
}
