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
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            var (success, message, redirectUrl) = await _userService.RegisterAsync(model);
            if (success)
            {
                return Json(new { success, message, redirectUrl });
            }
            return BadRequest(new { success, message });
        }


        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var (success, redirectUrl) = await _userService.LoginAsync(model);
            if (success)
            {
                return Json(new { success, redirectUrl });
            }
            return BadRequest(new { success, message = "Invalid credentials." });
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var success = await _userService.LogoutAsync();
            if (success)
            {
                Console.WriteLine("logged out!");
                return Json(new { success = true, redirectUrl = "/" });
            }
            return BadRequest(new { success = false, message = "Logout failed." });
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
