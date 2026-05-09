using GloboTicket.Services.Ordering.Entities;
using Microsoft.EntityFrameworkCore;

namespace GloboTicket.Services.Ordering.DbContexts;

public class OrderingDbContext : DbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(b =>
        {
            b.HasKey(o => o.OrderId);

            // Lines collection persisted into a JSON column on the Orders
            // row rather than a child table. The lines aren't edited
            // line-by-line — the row is written once when the order is
            // created — so the join table buys nothing.
            b.OwnsMany(o => o.Lines, lines =>
            {
                lines.ToJson();
            });
        });
    }
}
