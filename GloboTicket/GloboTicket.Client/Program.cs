using GloboTicket.Web.Models;
using GloboTicket.Web.Services;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var mvcBuilder = builder.Services.AddControllersWithViews();
if (builder.Environment.IsDevelopment())
    mvcBuilder.AddRazorRuntimeCompilation();

builder.Services.AddHttpClient<IEventCatalogService, EventCatalogService>(c =>
    c.BaseAddress = new Uri("https+http://eventcatalog"));
builder.Services.AddHttpClient<IShoppingBasketService, ShoppingBasketService>(c =>
    c.BaseAddress = new Uri("https+http://basket"));

// Typed client for the ordering service's status endpoint. Same
// pattern as the other typed clients above — MVC's default controller
// activator skips DI for the controller itself, so a typed wrapper
// service (resolved through DI) is what actually gets the configured
// HttpClient.
builder.Services.AddHttpClient<IOrderStatusClient, OrderStatusClient>(c =>
    c.BaseAddress = new Uri("https+http://ordering"));

builder.Services.AddSingleton<Settings>();

// Wolverine on the publishing side. We have no handlers in this project, so
// the only thing to set up is the RabbitMQ transport plus conventional
// routing — the same convention the Ordering service applies, which is what
// makes IMessageBus.InvokeAsync<OrderResult>(submitOrderCommand) reach the
// matching handler queue without either end naming an exchange explicitly.
//
// UseWolverine also registers IMessageBus in DI; that's what
// CheckoutController takes via constructor injection.
builder.Host.UseWolverine(opts =>
{
    var rabbitConn = builder.Configuration.GetConnectionString("rabbitmq")!;

    opts.UseRabbitMq(new Uri(rabbitConn))
        .AutoProvision()
        .UseConventionalRouting();
});

var app = builder.Build();

app.MapDefaultEndpoints();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=EventCatalog}/{action=Index}/{id?}");

app.Run();
