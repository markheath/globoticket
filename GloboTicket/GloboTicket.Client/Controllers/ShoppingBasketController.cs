using GloboTicket.Web.Extensions;
using GloboTicket.Web.Models;
using GloboTicket.Web.Models.Api;
using GloboTicket.Web.Models.View;
using GloboTicket.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GloboTicket.Web.Controllers
{
    public class ShoppingBasketController : Controller
    {
        private readonly IShoppingBasketService basketService;
        private readonly Settings settings;

        public ShoppingBasketController(IShoppingBasketService basketService, Settings settings)
        {
            this.basketService = basketService;
            this.settings = settings;
        }

        public async Task<IActionResult> Index()
        {
            var basketId = Request.Cookies.GetCurrentBasketId(settings);
            var basketLines = await basketService.GetLinesForBasket(basketId);
            var lineViewModels = basketLines.Select(bl => new BasketLineViewModel
            {
                LineId = bl.BasketLineId,
                EventId = bl.EventId,
                EventName = bl.Event.Name,
                Date = bl.Event.Date,
                Price = bl.Price,
                Quantity = bl.TicketAmount
            }).ToList();

            // The basket service is authoritative for the pricing summary
            // (subtotal, applied code, discount, total).
            var basket = await basketService.GetBasket(basketId);

            var viewModel = new BasketViewModel
            {
                Lines = lineViewModels,
                Subtotal = basket?.Subtotal ?? lineViewModels.Sum(l => l.Price * l.Quantity),
                DiscountCode = basket?.DiscountCode,
                DiscountAmount = basket?.DiscountAmount ?? 0,
                Total = basket?.Total ?? lineViewModels.Sum(l => l.Price * l.Quantity),
                DiscountError = TempData["DiscountError"] as string,
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyDiscount(string code)
        {
            var basketId = Request.Cookies.GetCurrentBasketId(settings);
            if (basketId != Guid.Empty && !string.IsNullOrWhiteSpace(code))
            {
                var (ok, error, _) = await basketService.ApplyDiscountCode(basketId, code.Trim());
                if (!ok)
                {
                    TempData["DiscountError"] = error;
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDiscount()
        {
            var basketId = Request.Cookies.GetCurrentBasketId(settings);
            if (basketId != Guid.Empty)
            {
                await basketService.RemoveDiscountCode(basketId);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLine(BasketLineForCreation basketLine)
        {
            var basketId = Request.Cookies.GetCurrentBasketId(settings);
            var newLine = await basketService.AddToBasket(basketId, basketLine);
            Response.Cookies.Append(settings.BasketIdCookieName, newLine.BasketId.ToString());

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLine(BasketLineForUpdate basketLineUpdate)
        {
            var basketId = Request.Cookies.GetCurrentBasketId(settings);
            await basketService.UpdateLine(basketId, basketLineUpdate);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveLine(Guid lineId)
        {
            var basketId = Request.Cookies.GetCurrentBasketId(settings);
            await basketService.RemoveLine(basketId, lineId);
            return RedirectToAction("Index");
        }

        public IActionResult Pay()
        {
            // Hand off to the checkout flow, which collects customer +
            // payment details and submits the order to the Ordering
            // service.
            return RedirectToAction("Index", "Checkout");
        }
    }
}
