using GloboTicket.Services.ShoppingBasket.Entities;

namespace GloboTicket.Services.ShoppingBasket;

// Single source of truth for basket totals. Prices are whole-dollar
// ints, so the percent case rounds to the nearest dollar and the fixed
// case is clamped so a discount can never push the total below zero.
public static class DiscountCalculator
{
    public static (int Subtotal, int Discount, int Total) Compute(
        IEnumerable<BasketLine> lines, DiscountCode? code)
    {
        var subtotal = lines.Sum(l => l.Price * l.TicketAmount);

        var discount = code switch
        {
            { Type: DiscountType.Percent } => (int)Math.Round(subtotal * code.Value / 100.0,
                MidpointRounding.AwayFromZero),
            { Type: DiscountType.Fixed } => Math.Min(code.Value, subtotal),
            _ => 0,
        };

        // Guard against a percent code configured above 100.
        discount = Math.Clamp(discount, 0, subtotal);

        return (subtotal, discount, subtotal - discount);
    }
}
