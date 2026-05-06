var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql");
var eventCatalogDb = sql.AddDatabase("eventcatalogdb");
var basketDb = sql.AddDatabase("basketdb");

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
