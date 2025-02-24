using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Base;

namespace TechXpress.Web.Controllers
{
    public class AdminController : Controller
    {
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View();
        }
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
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
    }
}
