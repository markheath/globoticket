using System.Net;
using GloboTicket.Messages.Ordering;
using GloboTicket.Web.Extensions;
using GloboTicket.Web.Models;
using GloboTicket.Web.Models.View;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using MessageOrderLine = GloboTicket.Messages.Ordering.OrderLine;

namespace GloboTicket.Web.Controllers;

// Drives the checkout UX. Index shows the form, Purchase publishes a
// SubmitOrderCommand to the ordering service over RabbitMQ and
// redirects to a status page that polls the ordering service via the
// OrderStatus action below.
public class CheckoutController : Controller
{
    private readonly IShoppingBasketService basketService;
    private readonly HttpClient orderingClient;
    private readonly IMessageBus bus;
    private readonly Settings settings;
    private readonly ILogger<CheckoutController> logger;

    public CheckoutController(
        IShoppingBasketService basketService,
        HttpClient orderingClient,
        IMessageBus bus,
        Settings settings,
        ILogger<CheckoutController> logger)
    {
        this.basketService = basketService;
        this.orderingClient = orderingClient;
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
    public async Task<IActionResult> Purchase(CheckoutViewModel checkout)
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
        await bus.PublishAsync(cmd);

        // Rotate the basket cookie so the next page load starts with an
        // empty basket. The previous basket row stays in the basket
        // service DB, orphaned but harmless.
        Response.Cookies.Delete(settings.BasketIdCookieName);

        return RedirectToAction(nameof(Order), new { orderId });
    }

    // Live status page. The page itself is static — it polls
    // OrderStatus below to render saga progress in real time.
    public IActionResult Order(Guid orderId)
    {
        ViewData["OrderId"] = orderId;
        return View();
    }

    // JSON pass-through to the ordering service's status endpoint.
    // Polled by the Order page; not consumed by any server-side code.
    [HttpGet]
    public async Task<IActionResult> OrderStatus(Guid orderId)
    {
        var response = await orderingClient.GetAsync($"/order/{orderId}/status");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        var body = await response.Content.ReadAsStringAsync();
        return Content(body, "application/json");
    }
}
