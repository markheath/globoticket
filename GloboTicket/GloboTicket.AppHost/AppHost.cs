var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql");
var eventCatalogDb = sql.AddDatabase("eventcatalogdb");
var basketDb = sql.AddDatabase("basketdb");

var storage = builder.AddAzureStorage("storage").RunAsEmulator();
var queues = storage.AddQueues("queues");

const string paymentQueueName = "paymentrequests";

var eventCatalog = builder.AddProject<Projects.GloboTicket_Services_EventCatalog>("eventcatalog")
    .WithReference(eventCatalogDb)
    .WaitFor(eventCatalogDb);

var basket = builder.AddProject<Projects.GloboTicket_Services_ShoppingBasket>("basket")
    .WithReference(basketDb)
    .WithReference(eventCatalog)
    .WaitFor(basketDb)
    .WaitFor(eventCatalog);

builder.AddProject<Projects.GloboTicket_Services_Payment>("payment")
    .WithReference(queues)
    .WithEnvironment("AzureQueues__QueueName", paymentQueueName)
    .WaitFor(queues);

builder.AddProject<Projects.GloboTicket_Web>("web")
    .WithReference(eventCatalog)
    .WithReference(basket)
    .WithReference(queues)
    .WithEnvironment("AzureQueues__QueueName", paymentQueueName)
    .WaitFor(eventCatalog)
    .WaitFor(basket);

builder.Build().Run();
