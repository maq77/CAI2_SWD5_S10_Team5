using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using TechXpress.Services.DTOs;
using TechXpress.Services;
using System.Linq;
using TechXpress.Services.Base;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    [Authorize(Policy = "Auth")]
    [Area("Customer")]
    //[Route("[area]/[controller]")]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public ProfileController(IUserService userService, IOrderService orderService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        }

        
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            SetPageMeta();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction(nameof(Login), "Account", new { area = "" });

            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null) return NotFound("User profile not found.");

            var orderHistory = await _orderService.GetOrdersByUserIdAsync(userId) ?? new List<OrderDTO>();

            var model = new CustomerProfileDTO
            {
                User = userProfile,
                OrderHistory = orderHistory ?? new List<OrderDTO>()
            };

            return View(model);
        }


        //[HttpGet("Edit")]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction(nameof(Login), "Account", new { area = "" });

            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null) return NotFound("User profile not found.");

            return View(userProfile);
        }


        //[HttpPost("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserProfileDTO model)
        {
            SetPageMeta();
            if (!ModelState.IsValid)
            {
                // Log errors for debugging
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("Validation Errors: " + string.Join(", ", errors));
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction(nameof(Login), "Account", new { area = "" });

            bool isUpdated = await _userService.UpdateUserProfileAsync(userId, model);
            if (!isUpdated)
            {
                ModelState.AddModelError("", "Failed to update profile.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Index));
        }


        
        [HttpGet("Orders/{userId}")]
        public async Task<IActionResult> GetOrdersByUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User ID is required.");

            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            if (orders == null || !orders.Any())
                return NotFound("No orders found for this user.");

            return Ok(orders);
        }

        
        private IActionResult Login()
        {
            return RedirectToAction("Login", "Account", new { area = "" });
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
