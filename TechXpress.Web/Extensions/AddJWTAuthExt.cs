using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechXpress.Data.Repositories;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Extensions
{
    public static class AuthenticationServiceExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure JWT settings
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
            //services.AddSingleton(jwtSettings);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));
                options.AddPolicy("Auth", policy =>
                    policy.RequireRole("Admin","Customer"));
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; // stores JWT in Cookie
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //Use it when testing jwt api
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.Cookie.HttpOnly = true; // js access
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // https only
                options.Cookie.SameSite = SameSiteMode.Strict; // CSRF attacks
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false, //has to be valid to make it true
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["AccessToken"];
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse(); // Suppress the default 401 response
                        var request = context.Request;
                        var returnUrl = request.Path + request.QueryString;

                        var loginUrl = $"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}";
                        context.Response.StatusCode = 302;
                        context.Response.Headers["Location"] = loginUrl; // Or wherever you want to redirect
                        return Task.CompletedTask;
                    },

                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 302;
                        var request = context.Request;
                        var returnUrl = request.Path + request.QueryString;

                        var accessDeniedUrl = $"/Account/AccessDenied?returnUrl={Uri.EscapeDataString(returnUrl)}";
                        context.Response.Headers["Location"] = accessDeniedUrl;
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}
