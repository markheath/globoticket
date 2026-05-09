using GloboTicket.Services.Ordering.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

// AddDbContextWithWolverineIntegration replaces the usual AddDbContext when
// we want SaveChangesAsync to flush Wolverine's outbox in the same
// transaction as our own writes. The DbContext registers the same way —
// Aspire's connection string injection still works.
builder.Services.AddDbContextWithWolverineIntegration<OrderingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("orderingdb"))
        // Same warning-suppression as the catalog: the migration-history
        // existence check is a try-SELECT-catch and EF logs the failed
        // SELECT at Error before the catch handles it. Demoting silences
        // the spurious dashboard error on a fresh DB.
        .ConfigureWarnings(w => w.Log((RelationalEventId.CommandError, LogLevel.Debug))));

// Wolverine wires itself up via UseWolverine on the host builder. There is no
// AddConsumer<T>() call — handlers are discovered by scanning this assembly
// for "Handler"/"Consumer" classes with a Handle/Consume method. (See
// NewOrderHandler.cs for what that scan is matching against.)
//
// UseConventionalRouting() then says: "for every message type my handlers
// accept, declare a RabbitMQ exchange named after the type and bind a queue
// for this service to it." That convention is what lets the Web project
// publish PaymentRequestMessageV2 without either side needing to share queue
// or exchange names — both ends apply the same convention to the same type.
//
// AutoProvision() creates the topology on startup; in production you'd
// usually pre-provision instead.
//
// UseEntityFrameworkCoreTransactions() opts in to the EF outbox: messages
// cascaded from a saga step (next phase of work) are persisted alongside
// any DbContext changes and dispatched after the commit succeeds.
builder.UseWolverine(opts =>
{
    var rabbitConn = builder.Configuration.GetConnectionString("rabbitmq")!;

    opts.UseRabbitMq(new Uri(rabbitConn))
        .AutoProvision()
        .UseConventionalRouting();

    opts.UseEntityFrameworkCoreTransactions();
});

var host = builder.Build();

// Apply EF migrations on startup. Same pattern as the catalog and basket
// services — a fresh AppHost run gets a clean orderingdb and the schema
// (including Wolverine's envelope tables) materialises before any handler
// runs.
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderingDbContext>>();
    logger.LogInformation("Applying Ordering migrations...");
    await db.Database.MigrateAsync();
    logger.LogInformation("Ordering migrations applied.");
}

await host.RunAsync();
