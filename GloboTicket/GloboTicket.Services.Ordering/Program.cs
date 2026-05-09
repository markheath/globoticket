using GloboTicket.Services.Ordering;
using GloboTicket.Services.Ordering.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddDbContext<OrderingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("orderingdb"))
        // Same warning-suppression as the catalog: the migration-history
        // existence check is a try-SELECT-catch and EF logs the failed
        // SELECT at Error before the catch handles it. Demoting silences
        // the spurious dashboard error on a fresh DB.
        .ConfigureWarnings(w => w.Log((RelationalEventId.CommandError, LogLevel.Debug))));

// Typed HttpClient against the catalog. SubmitOrderHandler injects
// ICatalogReservationsClient and uses it for both Reserve and Release.
builder.Services.AddHttpClient<ICatalogReservationsClient, CatalogReservationsClient>(c =>
    c.BaseAddress = new Uri("https+http://eventcatalog"));

// EmailSender wraps SMTP setup against the Mailpit endpoint Aspire
// injects via WithReference("mailpit"). Singleton because it's stateless.
builder.Services.AddSingleton<EmailSender>();

// Wolverine wires itself up via UseWolverine on the host builder. Handlers
// are discovered by scanning this assembly for "Handler"/"Consumer"
// classes with a Handle/Consume method (NewOrderHandler is the legacy
// compat artefact, SubmitOrderHandler is the live order path).
//
// UseConventionalRouting() declares a RabbitMQ exchange per message type
// and binds a queue for this service to it. The Web project publishes
// against the same convention without either side sharing queue or
// exchange names.
//
// SubmitOrderCommand is invoked via IMessageBus.InvokeAsync<OrderResult>
// from the Web project — Wolverine handles the request/response round-
// trip over RabbitMQ automatically when the handler returns a non-void
// type matching the caller's awaited generic argument.
builder.Host.UseWolverine(opts =>
{
    var rabbitConn = builder.Configuration.GetConnectionString("rabbitmq")!;

    opts.UseRabbitMq(new Uri(rabbitConn))
        .AutoProvision()
        .UseConventionalRouting();
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Apply EF migrations on startup. Same pattern as the catalog and basket
// services — a fresh AppHost run gets a clean orderingdb and the Orders
// table materialises before any handler runs.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<OrderingDbContext>>();
    logger.LogInformation("Applying Ordering migrations...");
    await db.Database.MigrateAsync();
    logger.LogInformation("Ordering migrations applied.");
}

app.MapControllers();

await app.RunAsync();
