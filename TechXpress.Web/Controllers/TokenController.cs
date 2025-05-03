using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class TokenController : Controller
    {
        private readonly ITokenService _tokenService;

        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDTO request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { success = false, message = "Refresh token is required" });
            }

            // Validate that the token belongs to the current user
            var tokenInfo = await _tokenService.GetTokenByRefreshToken(request.RefreshToken);
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (tokenInfo == null || (tokenInfo.UserId != userId && !User.IsInRole("Admin")))
            {
                return BadRequest(new { success = false, message = "Invalid token or unauthorized access" });
            }

            await _tokenService.RevokeToken(request.RefreshToken);
            return Ok(new { success = true, message = "Token has been revoked" });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RevokeAllTokens()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            await _tokenService.RevokeAllUserTokens(userId);
            return Ok(new { success = true, message = "All tokens have been revoked" });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateRefreshToken([FromBody] RefreshTokenDTO request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { success = false, message = "Refresh token is required" });
            }

            var isValid = await _tokenService.ValidateRefreshToken(request.RefreshToken);

            return Ok(new { success = true, isValid });
        }
    }
}