using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace GloboTicket.Services.ShoppingBasket.Entities;

public class Basket
{
    public Guid BasketId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    // The discount code currently applied to this basket, if any. Only
    // the code string is stored — the discount amount is always
    // recomputed from the current lines so it can't go stale when
    // quantities change.
    public string? DiscountCode { get; set; }

    public Collection<BasketLine> BasketLines { get; set; } = [];
}
