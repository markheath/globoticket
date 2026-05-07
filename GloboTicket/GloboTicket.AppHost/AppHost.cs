var builder = DistributedApplication.CreateBuilder(args);

// Postgres starts in a couple of seconds, so we deliberately don't use
// WithDataVolume() here — every AppHost run gets a clean database and the
// migrations + seed data re-apply on each start. That's slightly slower
// per-run but avoids the "half-applied migration on disk" class of bug
// that comes with persisted volumes during demo iteration.
var postgres = builder.AddPostgres("postgres");
var eventCatalogDb = postgres.AddDatabase("eventcatalogdb");
var basketDb = postgres.AddDatabase("basketdb");

var rabbit = builder.AddRabbitMQ("rabbitmq");

var eventCatalog = builder.AddProject<Projects.GloboTicket_Services_EventCatalog>("eventcatalog")
    .WithReference(eventCatalogDb)
    .WaitFor(eventCatalogDb);

var basket = builder.AddProject<Projects.GloboTicket_Services_ShoppingBasket>("basket")
    .WithReference(basketDb)
    .WithReference(eventCatalog)
    .WaitFor(basketDb)
    .WaitFor(eventCatalog);

builder.AddProject<Projects.GloboTicket_Services_Payment>("payment")
    .WithReference(rabbit)
    .WaitFor(rabbit);

builder.AddProject<Projects.GloboTicket_Web>("web")
    .WithReference(eventCatalog)
    .WithReference(basket)
    .WithReference(rabbit)
    .WaitFor(eventCatalog)
    .WaitFor(basket)
    .WaitFor(rabbit);

builder.Build().Run();
