using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Data.Model;
using TechXpress.Services.Base;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
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
            var orders = await _orderService.GetAllOrders();
            return View(orders);
        }
        [HttpGet]
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
        [HttpGet]
        public async Task<IActionResult> OrderSummary(int id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();
            return View(order);
        }
    }
}
