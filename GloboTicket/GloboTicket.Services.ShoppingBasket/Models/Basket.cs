namespace GloboTicket.Services.ShoppingBasket.Models
{
    public class Basket
    {
        public Guid BasketId { get; set; }
        public Guid UserId { get; set; }
        public int NumberOfItems { get; set; }

        // Pricing summary, computed server-side so the frontend doesn't
        // have to know the discount rules. DiscountCode is the applied
        // code (null if none); DiscountAmount is what it knocks off.
        public int Subtotal { get; set; }
        public string? DiscountCode { get; set; }
        public int DiscountAmount { get; set; }
        public int Total { get; set; }
    }
}
