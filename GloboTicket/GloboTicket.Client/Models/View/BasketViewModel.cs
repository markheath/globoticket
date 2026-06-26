namespace GloboTicket.Web.Models.View;

public class BasketViewModel
{
    public IEnumerable<BasketLineViewModel> Lines { get; set; } = [];
    public int Subtotal { get; set; }
    public string? DiscountCode { get; set; }
    public int DiscountAmount { get; set; }
    public int Total { get; set; }

    // Set when a just-submitted code was rejected, so the basket page can
    // show the customer why nothing changed.
    public string? DiscountError { get; set; }

    public bool HasDiscount => !string.IsNullOrEmpty(DiscountCode) && DiscountAmount > 0;
}
