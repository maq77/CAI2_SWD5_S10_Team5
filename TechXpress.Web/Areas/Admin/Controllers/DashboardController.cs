using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using TechXpress.ViewModels.Admin;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    
    [Area("Admin")]
    public class DashboardController : Controller
    {
        //[Authorize(Roles = "Admin")]
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public DashboardController(IProductService productService, IOrderService orderService, IUserService userService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["PageTitle"] = "Admin Dashboard";
            ViewData["BreadcrumbPath"] = new List<(string, string)>
            {
                ("/", "Admin")
            };

            var products = await _productService.GetAllProducts();
            var orders = await _orderService.GetAllOrders();
            var users = await _userService.GetAllUsersAsync();

            var dashboardData = new DashboardViewModel
            {
                TotalProducts = products.Count(),
                TotalOrders = orders.Count(),
                TotalUsers = users.Count(),
                TotalRevenue = orders.Sum(o => (double?)o.TotalAmount) ?? 0 // Handles null values
            };

            return View(dashboardData);
        }
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> AssignRole(string email, string role)
        {
            var result = await _userService.AssignRoleAsync(email, role);
            return RedirectToAction(nameof(Users));
        }

        public async Task<IActionResult> DeleteUser(string email)
        {
            await _userService.DeleteUserAsync(email);
            return RedirectToAction(nameof(Users));
        }

        #region Private Helper
        private async Task<IEnumerable<ProductDTO>> GetProducts()
        {
            var products = await _productService.GetAllProducts();
            
            return products;
        }
        private async Task<IEnumerable<OrderDTO>> GetOrders()
        {
            var orders = await _orderService.GetAllOrders();

            return orders;
        }

        private async Task<IEnumerable<UserDTO>> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            return users;
        }

        #endregion
    }
}
