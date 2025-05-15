using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Controllers
{
    //[Route("api/[controller]")]
    ///[ApiController]
    public class AccountController : Controller
    {

        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public AccountController(IUserService userService, ITokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }
        [HttpGet]
        public async Task<IActionResult> WhoAmI()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Json(claims);
        }

        public IActionResult Register() => View();
        public IActionResult AccessDenied() => View();

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });
            }
            var (authResponse, redirectUrl) = await _userService.RegisterAsync(model);
            if (authResponse.IsSuccess)
            {
                return Json(new { authResponse.IsSuccess, authResponse.Message, authResponse.Token, authResponse.RefreshToken, redirectUrl });
            }
            return BadRequest(new { authResponse.IsSuccess, authResponse.Message});
        }


        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //[HttpPost("login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid input data", errors = ModelState });
            }
            var stopwatch = Stopwatch.StartNew(); ///for testing performance

            var (authResponse, redirectUrl) = await _userService.LoginAsync(model);

            stopwatch.Stop(); ///
            Console.WriteLine($"Login took: {stopwatch.ElapsedMilliseconds} ms");
            if (authResponse.IsSuccess)
            {
                // Set access token in cookie (short-lived)
                Response.Cookies.Append("AccessToken", authResponse.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(3) // Short-lived token
                });

                // Set refresh token in cookie (longer-lived)
                Response.Cookies.Append("RefreshToken", authResponse.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7) // 1-week refresh token
                });
                if (!string.IsNullOrEmpty(model.returnUrl) && Url.IsLocalUrl(model.returnUrl))
                {
                    redirectUrl = model.returnUrl;
                }
                return Ok(new { authResponse.IsSuccess, authResponse.Message, authResponse.Token, authResponse.RefreshToken, redirectUrl });
            }
            return BadRequest(new { authResponse.IsSuccess, authResponse.Message });
        }


        //[HttpPost("logout")]
        [Authorize]
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

        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO model)
        {
            if (string.IsNullOrEmpty(model.RefreshToken))
            {
                return BadRequest(new { success = false, message = "Invalid refresh token. Require Token" });
            }
            var authResponse = await _userService.RefreshTokenAsync(model.RefreshToken);
            if (authResponse.IsSuccess)
            {
                return Json(new { authResponse.IsSuccess, authResponse.Message, authResponse.Token, authResponse.RefreshToken });
            }
            return BadRequest(new { authResponse.IsSuccess, authResponse.Message });
        }
        
    }
}
