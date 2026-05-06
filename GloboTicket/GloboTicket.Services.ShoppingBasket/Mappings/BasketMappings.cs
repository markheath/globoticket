using GloboTicket.Services.ShoppingBasket.Models;

namespace GloboTicket.Services.ShoppingBasket.Mappings;

public static class BasketMappings
{
    public static Basket ToModel(this Entities.Basket basket) => new()
    {
        BasketId = basket.BasketId,
        UserId = basket.UserId,
    };

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
