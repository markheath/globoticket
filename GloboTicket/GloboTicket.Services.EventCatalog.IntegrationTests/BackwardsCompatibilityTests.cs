using System.Text.Json;
using GloboTicket.Services.EventCatalog.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GloboTicket.Services.EventCatalog.IntegrationTests;

public class BackwardsCompatibilityTests : IClassFixture<EventCatalogFactory>
{
    private readonly HttpClient _client;

    public BackwardsCompatibilityTests(EventCatalogFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Version1ClientsCanGetEvents()
    {
        var json = await _client.GetStringAsync("/api/events");
        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.GetArrayLength() > 0, "Got no events back");
        var firstEvent = document.RootElement[0];
        Assert.True(firstEvent.TryGetProperty("price", out _));
        Assert.False(firstEvent.TryGetProperty("tickets", out _));
    }

    [Fact]
    public async Task Version2ClientsCanGetEvents()
    {
        var json = await _client.GetStringAsync("/api/events?api-version=2.0");
        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.GetArrayLength() > 0, "Got no events back");
        var firstEvent = document.RootElement[0];
        Assert.False(firstEvent.TryGetProperty("price", out _));
        Assert.True(firstEvent.TryGetProperty("tickets", out _));
    }
}

public class EventCatalogFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<EventCatalogDbContext>));

            // Isolate the InMemory provider in its own service provider so it doesn't
            // collide with the Npgsql services registered by Program.cs.
            var inMemoryServices = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<EventCatalogDbContext>(options =>
                options.UseInMemoryDatabase("EventCatalogTests")
                       .UseInternalServiceProvider(inMemoryServices));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventCatalogDbContext>();
        db.Database.EnsureCreated();
        return host;
    }
}
