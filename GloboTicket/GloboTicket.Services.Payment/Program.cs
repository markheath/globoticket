using GloboTicket.Services.Payment;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NewOrderHandler>();
    x.AddConsumer<NewOrderHandlerV2>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq")!);
        cfg.ConfigureEndpoints(context);
    });
});

await builder.Build().RunAsync();
