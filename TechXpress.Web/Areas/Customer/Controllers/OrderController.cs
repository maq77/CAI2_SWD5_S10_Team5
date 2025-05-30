﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe.Climate;
using TechXpress.Data.Enums;
using TechXpress.Data.Model;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    [Authorize(Policy = "Auth")]
    [Area("Customer")]
    public class OrderController : Controller
    {

        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<User> _userManager;
        public OrderController(IOrderService orderService, UserManager<User> userManager, ICartService cartService)
        {
            _orderService = orderService;
            _userManager = userManager;
            _cartService = cartService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            SetPageMeta();
            var user = await _userManager.GetUserAsync(User);
            var orders = await _orderService.GetOrdersByUserIdAsync(user.Id);
            return View(orders);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            SetPageMeta();
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            return View(order);
        }


        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteOrder(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> OrderSummary(int id)
        {
            SetPageMeta();
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            return View(order);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromCart()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var cartItems = _cartService.GetCart();
            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            var orderDto = new OrderDTO
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                shipping_address = "DUMP Data", 
                OrderDetails = cartItems
                
            };

            int order_id = await _orderService.CreateOrder(orderDto);

            if (order_id == 0)
            {
                ModelState.AddModelError("", "Failed to create order.");
                return RedirectToAction("Index", "Cart");
            }
            _cartService.ClearCart();

            return RedirectToAction("OrderSummary", new { id = order_id });

        }
        #region Private Helper Methods

        //
        protected void SetPageMeta()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            ViewData["PageTitle"] = FormatTitle(action);
            ViewData["BreadcrumbPath"] = new List<(string, string)>
        {
            ("/", "Home"),
            ($"/{controller}", FormatTitle(controller))
        };
        }

        private string FormatTitle(string text) =>
            System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.Replace("_", " ").ToLower());
        #endregion

    }
}
