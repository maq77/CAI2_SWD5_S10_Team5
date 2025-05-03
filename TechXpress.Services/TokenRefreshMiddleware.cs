using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.Base;

namespace TechXpress.Services
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService, ITokenService tokenService)
        {
            // Check if access token exists and is valid
            if (context.Request.Cookies.TryGetValue("AccessToken", out string accessToken))
            {
                // If token is about to expire or invalid, try to refresh
                if (await tokenService.IsTokenExpiring(accessToken))
                {
                    await RefreshTokens(context, userService);
                }
            }
            // No access token but refresh token exists - try to refresh
            else if (context.Request.Cookies.ContainsKey("RefreshToken"))
            {
                await RefreshTokens(context, userService);
            }

            await _next(context);
        }

        private async Task RefreshTokens(HttpContext context, IUserService userService)
        {
            // Try to get refresh token
            if (context.Request.Cookies.TryGetValue("RefreshToken", out string refreshToken))
            {
                var authResponse = await userService.RefreshTokenAsync(refreshToken);

                if (authResponse.IsSuccess)
                {
                    // Set new tokens
                    context.Response.Cookies.Append("AccessToken", authResponse.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(30)
                    });

                    context.Response.Cookies.Append("RefreshToken", authResponse.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(14)
                    });
                }
                else
                {
                    // Clear cookies if refresh failed - will force re-login
                    context.Response.Cookies.Delete("AccessToken");
                    context.Response.Cookies.Delete("RefreshToken");

                    // Only redirect if not API call
                    if (!context.Request.Path.StartsWithSegments("/api") &&
                        !context.Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
                    {
                        context.Response.Redirect("/Account/Login");
                    }
                }
            }
        }
    }
}
