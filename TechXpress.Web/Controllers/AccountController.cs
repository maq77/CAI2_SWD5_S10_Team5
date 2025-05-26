using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using TechXpress.Services.DTOs.ViewModels;

namespace TechXpress.Web.Controllers
{
    //[Route("api/[controller]")]
    ///[ApiController]
    public class AccountController : Controller
    {

        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IEmailServer _emailServer;

        public AccountController(IUserService userService, ITokenService tokenService, IEmailServer emailServer)
        {
            _userService = userService;
            _tokenService = tokenService;
            _emailServer = emailServer;
        }
        [HttpGet]
        public async Task<IActionResult> WhoAmI()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Json(claims);
        }

        public IActionResult Register() => View();
        public IActionResult ForgotPassword() => View();
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ResendConfirmationEmail()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User not Auth");
            var authResponse = await _userService.SendEmailConfirmation(userId);
            if (authResponse.IsSuccess)
            {
                return Json(new { authResponse.IsSuccess, authResponse.Message, authResponse.Token, authResponse.RefreshToken});
            }
            return BadRequest(new { authResponse.IsSuccess, authResponse.Message });
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

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest("Missing email or token.");

            var user = await _userService.FindUserByEmail(email);
            if (user == null)
                return NotFound("User not found.");

            var result = await _userService.ConfirmEmail(user, token);

            if (result.Succeeded)
            {
                return View("ConfirmEmailSuccess");
            }

            return View("ConfirmEmailFailure");
        }


        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userService.FindUserByEmail(email);
            if (user == null)
            {
                // Do not reveal that the user does not exist
                return View("ForgotPasswordConfirmation");
            }

            var token = await _userService.GeneratePasswordResetToken(user);
            var callbackUrl = Url.Action("ResetPassword", "Account",
                new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

            await _emailServer.SendEmailAsync(
                email,
                "Reset Your Password",
                $"Please reset your password by <a href='{callbackUrl}'>clicking here</a>.");

            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            var model = new ResetPasswordViewModel { UserId = userId, Token = token };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userService.FindUserById(model.UserId);
            if (user == null) return View("ResetPasswordConfirmation");

            var result = await _userService.ResetPassword(user, model.Token, model.Password);
            if (result.Succeeded) return View("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
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
