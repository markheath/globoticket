using NUnit.Framework;
using System.Text.Json;

namespace GloboTicket.Services.EventCatalog.IntegrationTests
{
    public class Tests
    {
        private HttpClient httpClient = null!;

        [SetUp]
        public void Setup()
        {
            var catalogServiceUrl = Environment.GetEnvironmentVariable("CATALOG_SERVICE")
                ?? "https://localhost:5001/";
            httpClient = new HttpClient { BaseAddress = new Uri(catalogServiceUrl) };
        }

        [Test]
        public async Task Version1ClientsCanGetEvents()
        {
            var json = await httpClient.GetStringAsync("/api/events");
            using var document = JsonDocument.Parse(json);
            var events = document.RootElement;
            Assert.That(events.GetArrayLength(), Is.GreaterThan(0), "Got no events back");
            var firstEvent = events[0];
            Assert.That(firstEvent.TryGetProperty("price", out _), Is.True);
            Assert.That(firstEvent.TryGetProperty("tickets", out _), Is.False);
        }

        [Test]
        public async Task Version2ClientsCanGetEvents()
        {
            var json = await httpClient.GetStringAsync("/api/events?api-version=2.0");
            using var document = JsonDocument.Parse(json);
            var events = document.RootElement;
            Assert.That(events.GetArrayLength(), Is.GreaterThan(0), "Got no events back");
            var firstEvent = events[0];
            Assert.That(firstEvent.TryGetProperty("price", out _), Is.False);
            Assert.That(firstEvent.TryGetProperty("tickets", out _), Is.True);
        }
    }
}
