using GloboTicket.Messages;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Tracking;
using Xunit;

namespace GloboTicket.Services.Payment.Tests;

// These tests are the contract guard for the message-versioning lesson: if
// someone deletes NewOrderHandler (V1) thinking V2 has fully replaced it,
// the V1 test will fail, because TrackActivity will time out waiting for a
// handler that no longer exists.
//
// No RabbitMQ transport is configured here. Wolverine still routes messages
// to handlers in-process — the transport only matters once you cross a
// process boundary — so these tests run in a fraction of a second without
// any container.
public class PaymentBackwardsCompatibilityTests
{
    [Fact]
    public async Task V1_publishers_are_consumed_by_NewOrderHandler()
    {
        using var host = await BuildHost();

        var session = await host.TrackActivity()
            .ExecuteAndWaitAsync(bus =>
                bus.PublishAsync(new PaymentRequestMessage { BasketId = Guid.NewGuid() }));

        Assert.Single(session.Executed.MessagesOf<PaymentRequestMessage>());
    }

    [Fact]
    public async Task V2_publishers_are_consumed_by_NewOrderHandlerV2()
    {
        using var host = await BuildHost();

        var session = await host.TrackActivity()
            .ExecuteAndWaitAsync(bus =>
                bus.PublishAsync(new PaymentRequestMessageV2 { OrderId = Guid.NewGuid() }));

        Assert.Single(session.Executed.MessagesOf<PaymentRequestMessageV2>());
    }

    // Discovery is what makes the magic work: we point Wolverine at the
    // Payment assembly and it scans for handler classes — same as the real
    // service does at startup. Nothing else has to be registered.
    private static async Task<IHost> BuildHost()
    {
        var builder = Host.CreateApplicationBuilder();
        builder.UseWolverine(opts =>
            opts.Discovery.IncludeAssembly(typeof(NewOrderHandler).Assembly));
        var host = builder.Build();
        await host.StartAsync();
        return host;
    }
}
