using GloboTicket.Messages;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GloboTicket.Services.Payment.Tests;

public class PaymentBackwardsCompatibilityTests
{
    [Fact]
    public async Task V1_publishers_are_consumed_by_NewOrderHandler()
    {
        await using var provider = BuildHarness();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new PaymentRequestMessage { BasketId = Guid.NewGuid() });

        Assert.True(await harness.Consumed.Any<PaymentRequestMessage>());

        var consumerHarness = provider.GetRequiredService<IConsumerTestHarness<NewOrderHandler>>();
        Assert.True(await consumerHarness.Consumed.Any<PaymentRequestMessage>());
    }

    [Fact]
    public async Task V2_publishers_are_consumed_by_NewOrderHandlerV2()
    {
        await using var provider = BuildHarness();
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new PaymentRequestMessageV2 { OrderId = Guid.NewGuid() });

        Assert.True(await harness.Consumed.Any<PaymentRequestMessageV2>());

        var consumerHarness = provider.GetRequiredService<IConsumerTestHarness<NewOrderHandlerV2>>();
        Assert.True(await consumerHarness.Consumed.Any<PaymentRequestMessageV2>());
    }

    private static ServiceProvider BuildHarness() =>
        new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<NewOrderHandler>();
                x.AddConsumer<NewOrderHandlerV2>();
            })
            .BuildServiceProvider(true);
}
