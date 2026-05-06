using GloboTicket.Services.Payment;
using Microsoft.Azure.Storage;
using Rebus.Activation;
using Rebus.Config;

var builder = Host.CreateApplicationBuilder(args);
var host = builder.Build();

var storageAccount = CloudStorageAccount.Parse(builder.Configuration["AzureQueues:ConnectionString"]);

using var activator = new BuiltinHandlerActivator();
activator.Register(() => new NewOrderHandler());
activator.Register(() => new NewOrderHandlerV2());
Configure.With(activator)
    .Transport(t => t.UseAzureStorageQueues(storageAccount, builder.Configuration["AzureQueues:QueueName"]))
    .Start();

await host.RunAsync();
