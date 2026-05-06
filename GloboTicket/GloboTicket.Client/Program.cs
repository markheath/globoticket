using GloboTicket.Messages;
using GloboTicket.Web.Models;
using GloboTicket.Web.Services;
using Microsoft.Azure.Storage;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.ServiceProvider;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
    mvcBuilder.AddRazorRuntimeCompilation();

builder.Services.AddHttpClient<IEventCatalogService, EventCatalogService>(c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiConfigs:EventCatalog:Uri"]!));
builder.Services.AddHttpClient<IShoppingBasketService, ShoppingBasketService>(c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiConfigs:ShoppingBasket:Uri"]!));

builder.Services.AddSingleton<Settings>();

var storageAccount = CloudStorageAccount.Parse(builder.Configuration["AzureQueues:ConnectionString"]);

builder.Services.AddRebus(c => c
    .Transport(t => t.UseAzureStorageQueuesAsOneWayClient(storageAccount))
    .Routing(r => r.TypeBased()
        .Map<PaymentRequestMessage>(builder.Configuration["AzureQueues:QueueName"])
        .Map<PaymentRequestMessageV2>(builder.Configuration["AzureQueues:QueueName"]))
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.Services.UseRebus();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=EventCatalog}/{action=Index}/{id?}");

app.Run();
