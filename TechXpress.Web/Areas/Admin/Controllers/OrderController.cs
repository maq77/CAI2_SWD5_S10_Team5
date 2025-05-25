using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TechXpress.Data.Enums;
using TechXpress.Data.Model;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs.ViewModels;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Authorize(Policy = "AdminOnly")]
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
            var model = new List<OrderViewModel>();

            foreach (var order in orders)
            {
                // Retrieve the user details using UserManager
                var user = await _userManager.FindByIdAsync(order.UserId);

                model.Add(new OrderViewModel
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    TotalAmount = order.TotalAmount,
                    OrderDate = order.OrderDate,
                    SelectedStatus = order.Status,
                    shipping_address = order.shipping_address,
                    paymentMethod = order.paymentMethod,
                    transactionId = order.TransactionId ?? "NA",
                    CustomerName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    CustomerEmail = user?.Email ?? "Unknown",
                    Items = order.OrderDetails.Select(item => new OrderDetailViewModel
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        ProductName = item.Product.Name ?? "Unknown"
                    }).ToList()
                });
            }
            return View(model);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromBody] OrderStatusUpdateViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var result = await _orderService.UpdateOrderStatus(model.OrderId, model.Status);
                if (!result)
                {
                    return NotFound("Order not found or status could not be updated.");
                }
                return Ok(new { success = true, message = "Order status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An error occurred while updating the order status." });
            }
        }

    }
}
