using GloboTicket.Messages.Ordering;
using GloboTicket.Web.Extensions;
using GloboTicket.Web.Models;
using GloboTicket.Web.Models.View;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using MessageOrderLine = GloboTicket.Messages.Ordering.OrderLine;

namespace GloboTicket.Web.Controllers;

// Drives the checkout UX. Index shows the form; Purchase invokes
// SubmitOrderCommand on the ordering service over RabbitMQ as a
// request/response (Wolverine's IMessageBus.InvokeAsync) and waits for
// the OrderResult before redirecting. By the time we redirect, the
// order is in a terminal state (Confirmed or Failed) — Order then just
// reads it and renders the result.
public class CheckoutController : Controller
{
    private readonly IShoppingBasketService basketService;
    private readonly IOrderStatusClient orderStatusClient;
    private readonly IMessageBus bus;
    private readonly Settings settings;
    private readonly ILogger<CheckoutController> logger;

    public CheckoutController(
        IShoppingBasketService basketService,
        IOrderStatusClient orderStatusClient,
        IMessageBus bus,
        Settings settings,
        ILogger<CheckoutController> logger)
    {
        this.basketService = basketService;
        this.orderStatusClient = orderStatusClient;
        this.bus = bus;
        this.settings = settings;
        this.logger = logger;
    }

    // Pre-filled with demo-friendly values so the form is one-click for a
    // walkthrough. The card defaults to the success preset; the dropdown
    // in the view lets the demo switch to the "ends in 0000" decline.
    public IActionResult Index() => View(new CheckoutViewModel
    {
        Name = "A Customer",
        Email = "customer@example.com",
        Address = "123 Example Street",
        Town = "Wolverton",
        PostalCode = "WV1 2PR",
        CreditCard = "4242424242424242",
        CreditCardDate = "10/26",
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Purchase(CheckoutViewModel checkout, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Index), checkout);
        }

        var basketId = Request.Cookies.GetCurrentBasketId(settings);
        var lines = (await basketService.GetLinesForBasket(basketId)).ToList();
        if (lines.Count == 0)
        {
            return RedirectToAction("Index", "ShoppingBasket");
        }

        var orderId = Guid.NewGuid();
        var cmd = new SubmitOrderCommand(
            orderId,
            new CustomerDetails(
                checkout.Name, checkout.Email,
                checkout.Address, checkout.Town, checkout.PostalCode),
            [.. lines.Select(l => new MessageOrderLine(
                l.EventId, l.Event.Name, l.Event.Artist,
                l.TicketAmount, l.Price))],
            checkout.CreditCard,
            checkout.CreditCardDate);

        logger.LogInformation("Submitting order {OrderId} for {Customer}",
            orderId, checkout.Name);

        // Request/response over RabbitMQ. Blocks until SubmitOrderHandler
        // on the ordering service has either confirmed or failed the order.
        var result = await bus.InvokeAsync<OrderResult>(cmd, ct);
        logger.LogInformation("Order {OrderId} resolved: confirmed={Confirmed} reason={Reason}",
            orderId, result.Confirmed, result.FailureReason);

        // Rotate the basket cookie so the next page load starts with an
        // empty basket. The previous basket row stays in the basket
        // service DB, orphaned but harmless.
        Response.Cookies.Delete(settings.BasketIdCookieName);

        return RedirectToAction(nameof(Order), new { orderId });
    }

    // Result page. The order is already in a terminal state (Confirmed
    // or Failed) by the time we get here, so we fetch it once from the
    // ordering service and render the outcome server-side. No polling.
    public async Task<IActionResult> Order(Guid orderId)
    {
        var status = await orderStatusClient.GetStatus(orderId);
        if (status is null)
        {
            return NotFound();
        }

        return View(new OrderResultViewModel(orderId, status.Status, status.FailureReason));
    }
}
