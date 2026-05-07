using GloboTicket.Services.ShoppingBasket.DbContexts;
using GloboTicket.Services.ShoppingBasket.Repositories;
using GloboTicket.Services.ShoppingBasket.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IBasketLinesRepository, BasketLinesRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddHttpClient<IEventCatalogService, EventCatalogService>(c =>
    c.BaseAddress = new Uri("https+http://eventcatalog"));

builder.Services.AddDbContext<ShoppingBasketDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("basketdb"))
        // See EventCatalog Program.cs for the rationale — same Npgsql
        // first-run probe noise, same fix.
        .ConfigureWarnings(w => w.Log((RelationalEventId.CommandError, LogLevel.Debug))));

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ShoppingBasketDbContext>();
    app.Logger.LogInformation("Applying ShoppingBasket migrations...");
    await db.Database.MigrateAsync();
    app.Logger.LogInformation("ShoppingBasket migrations applied.");
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
