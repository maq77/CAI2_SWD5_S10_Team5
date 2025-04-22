using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechXpress.Data.Enums;
using TechXpress.Data.Model;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    [Authorize]
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

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrders();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            return View(order);
        }


        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteOrder(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> OrderSummary(int id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            return View(order);
        }
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
                OrderDetails = cartItems
                
            };

            await _orderService.CreateOrder(orderDto);
            _cartService.ClearCart();

            return RedirectToAction("OrderSummary", new { id = orderDto.Id });
        }

    }
}
