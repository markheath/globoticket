using GloboTicket.Services.ShoppingBasket.Entities;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.ShoppingBasket.DbContexts
{
    public class ShoppingBasketDbContext : DbContext
    {
        public ShoppingBasketDbContext(DbContextOptions<ShoppingBasketDbContext> options)
        : base(options)
        {
        }

        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketLine> BasketLines { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Event.Date is a calendar date copied across from the catalog
            // service. Same Npgsql timestamp/timestamptz quirk as the catalog
            // service handles — see EventCatalogDbContext for the rationale.
            modelBuilder.Entity<Event>()
                .Property(e => e.Date)
                .HasColumnType("timestamp without time zone");
        }
    }
}
