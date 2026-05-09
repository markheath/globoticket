using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

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
builder.UseWolverine(opts =>
{
    var rabbitConn = builder.Configuration.GetConnectionString("rabbitmq")!;

    opts.UseRabbitMq(new Uri(rabbitConn))
        .AutoProvision()
        .UseConventionalRouting();
});

await builder.Build().RunAsync();
