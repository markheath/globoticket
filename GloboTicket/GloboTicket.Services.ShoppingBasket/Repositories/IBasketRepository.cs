using GloboTicket.Services.ShoppingBasket.Entities;

namespace GloboTicket.Services.ShoppingBasket.Repositories;

public interface IBasketRepository
{
    Task<bool> BasketExists(Guid basketId);
    Task<Basket?> GetBasketById(Guid basketId);
    void AddBasket(Basket basket);
    Task<bool> SaveChanges();
}
