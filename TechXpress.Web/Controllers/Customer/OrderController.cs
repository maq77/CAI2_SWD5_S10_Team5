using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechXpress.Data.Enums;
using TechXpress.Data.Model;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Controllers.Customer
{
    //[Authorize]
    public class OrderController : Controller
    {

        private readonly IOrderService _orderService;
        private readonly UserManager<User> _userManager;
        public OrderController(IOrderService orderService, UserManager<User> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
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

        // Display Checkout Form
        public IActionResult Create()
        {
            return View(new OrderDTO()); //  Pass empty order DTO to the form
        }
        [HttpPost]
        public async Task<IActionResult> Create(OrderDTO orderDto)
        {
            if (!ModelState.IsValid) return View(orderDto);

            var user = await _userManager.GetUserAsync(User); //  Get logged-in user
            if (user == null) return Unauthorized();

            orderDto.UserId = user.Id; // Assign order to the logged-in user
            orderDto.OrderDate = DateTime.UtcNow;
            orderDto.Status = OrderStatus.Pending;

            var result = await _orderService.CreateOrder(orderDto);

            if (!result)
            {
                ModelState.AddModelError("", "Error processing order. Try again.");
                return View(orderDto);
            }

            return RedirectToAction("OrderSummary", new { id = orderDto.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderService.GetOrderById(id);
            if (order == null) return NotFound();

            ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(OrderStatus)));
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OrderDTO orderDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(OrderStatus)));
                return View(orderDto);
            }

            var result = await _orderService.UpdateOrder(orderDto);
            if (!result)
            {
                ModelState.AddModelError("", "Failed to update order.");
                return View(orderDto);
            }

            return RedirectToAction(nameof(Index));
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

            //var cartItems = _cartService.GetCart();
            //if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            var orderDto = new OrderDTO
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                // Items = cartItems
            };

            await _orderService.CreateOrder(orderDto);
            //_cartService.ClearCart(); // ✅ Empty cart after checkout

            return RedirectToAction("OrderSummary", new { id = orderDto.Id });
        }

    }
}
