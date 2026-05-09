using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GloboTicket.Services.Ordering.DbContexts;

// Used only by `dotnet ef migrations add` / `database update` at design
// time. Without this, EF would build the full Generic Host to resolve
// the DbContext, and that fails because Wolverine's UseRabbitMq is
// configured with a non-null connection URI that Aspire only injects at
// run time.
//
// The connection string here is a placeholder — `dotnet ef migrations`
// only needs to scaffold SQL, never to open a connection.
public class OrderingDbContextDesignTimeFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<OrderingDbContext>()
            .UseNpgsql("Host=localhost;Database=orderingdb;Username=postgres;Password=postgres")
            .Options;
        return new OrderingDbContext(options);
    }
}
