using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GloboTicket.Services.EventCatalog.IntegrationTests
{
    public class Tests
    {
        private HttpClient httpClient;
        [SetUp]
        public void Setup()
        {
            var catalogServiceUrl = Environment.GetEnvironmentVariable("CATALOG_SERVICE");
            if (String.IsNullOrEmpty(catalogServiceUrl))
            {
                catalogServiceUrl = "https://localhost:5001/";
            }
            httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(catalogServiceUrl);
        }

        [Test]
        public async Task Version1ClientsCanGetEvents()
        {
            var json = await httpClient.GetStringAsync("/api/events");
            var events = JArray.Parse(json);
            Assert.That(events.Count, Is.GreaterThan(0), "Got no events back");
            var firstEvent = events[0];
            Assert.IsNotNull(firstEvent["price"]);
            Assert.IsNull(firstEvent["tickets"]);
        }


        [Test]
        public async Task Version2ClientsCanGetEvents()
        {
            var result = await httpClient.GetStringAsync("/api/events?api-version=2.0");
            var events = JArray.Parse(result);
            Assert.That(events.Count, Is.GreaterThan(0), "Got no events back");
            var firstEvent = events[0];
            Assert.IsNull(firstEvent["price"]);
            Assert.IsNotNull(firstEvent["tickets"]);
        }
    }
}