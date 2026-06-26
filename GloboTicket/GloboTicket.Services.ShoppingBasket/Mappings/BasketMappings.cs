using GloboTicket.Services.ShoppingBasket.Models;

namespace GloboTicket.Services.ShoppingBasket.Mappings;

public static class BasketMappings
{
    public static Basket ToModel(this Entities.Basket basket) => new()
    {
        BasketId = basket.BasketId,
        UserId = basket.UserId,
    };

    // Maps the basket plus its pricing summary. The resolved discount
    // code (already validated/active, or null) is passed in so the
    // amounts come from the single DiscountCalculator source of truth.
    public static Basket ToModelWithSummary(this Entities.Basket basket, Entities.DiscountCode? discount)
    {
        var (subtotal, discountAmount, total) = DiscountCalculator.Compute(basket.BasketLines, discount);
        return new Basket
        {
            BasketId = basket.BasketId,
            UserId = basket.UserId,
            NumberOfItems = basket.BasketLines.Sum(bl => bl.TicketAmount),
            Subtotal = subtotal,
            DiscountCode = discount?.Code,
            DiscountAmount = discountAmount,
            Total = total,
        };
    }

    public static Entities.Basket ToEntity(this BasketForCreation basket) => new()
    {
        UserId = basket.UserId,
    };

    public static BasketLine ToModel(this Entities.BasketLine line) => new()
    {
        BasketLineId = line.BasketLineId,
        BasketId = line.BasketId,
        EventId = line.EventId,
        Price = line.Price,
        TicketAmount = line.TicketAmount,
        Event = line.Event?.ToModel()!,
    };

    public static Entities.BasketLine ToEntity(this BasketLineForCreation line) => new()
    {
        EventId = line.EventId,
        Price = line.Price,
        TicketAmount = line.TicketAmount,
    };

    public static void ApplyTo(this BasketLineForUpdate update, Entities.BasketLine entity)
    {
        entity.TicketAmount = update.TicketAmount;
    }

    public static Event ToModel(this Entities.Event @event) => new()
    {
        EventId = @event.EventId,
        Name = @event.Name,
        Date = @event.Date,
    };
}
