using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var (isSuccess, message) = await _userService.RegisterAsync(model);

            if (isSuccess)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", message);
            return View(model);
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var redirectUrl = await _userService.LoginAsync(model);
            if (redirectUrl != null)
                return Redirect(redirectUrl); 

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
