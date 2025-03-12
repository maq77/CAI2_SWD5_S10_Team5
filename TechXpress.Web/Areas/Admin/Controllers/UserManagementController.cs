using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]  // Only admins can manage users
    public class UserManagementController : Controller
    {
        private readonly IUserService _userService;

        public UserManagementController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        public async Task<IActionResult> AssignRole(string email, string role)
        {
            var success = await _userService.AssignRoleAsync(email, role);
            if (!success)
            {
                TempData["Error"] = "Failed to assign role.";
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> MakeAdmin(string email)
        {
            var success = await _userService.MakeUserAdmin(email);
            if (!success)
            {
                TempData["Error"] = "Failed to assign role.";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteUser(string email)
        {
            var success = await _userService.DeleteUserAsync(email);
            if (!success)
            {
                TempData["Error"] = "Failed to delete user.";
            }
            return RedirectToAction("Index");
        }
    }
}
