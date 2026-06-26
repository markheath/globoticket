namespace GloboTicket.Services.ShoppingBasket.Entities;

// A promotional code a customer can apply to a basket. Owned by the
// basket service; the catalog of codes lives in the DiscountCodes table
// and is editable directly in the DB.
public class DiscountCode
{
    // The code the customer types, e.g. "SUMMER10". Primary key, matched
    // case-insensitively at lookup time.
    public string Code { get; set; } = string.Empty;

    public DiscountType Type { get; set; }

    // Percent off (1–100) when Type == Percent, or a flat dollar amount
    // when Type == Fixed.
    public int Value { get; set; }

    // Inactive codes are kept for history but no longer apply.
    public bool IsActive { get; set; } = true;
}

public enum DiscountType
{
    Percent,
    Fixed,
}
