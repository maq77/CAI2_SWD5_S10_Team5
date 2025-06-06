﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TechXpress.Data.Constants;
using TechXpress.Data.Model;

namespace TechXpress.Data
{
    public class DbSeeder
    {
        public static async Task AddMigrations(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate(); // or db.Database.EnsureCreated();
            }

            return;
        }

        public static async Task SeedData(IApplicationBuilder app)
        {
            // Create a scoped service provider to resolve dependencies
            using var scope = app.ApplicationServices.CreateScope();

            // resolve the logger service
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbSeeder>>();

            try
            {
                // resolve other dependencies
                var userManager = scope.ServiceProvider.GetService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();

                // Check if any users exist to prevent duplicate seeding
                if (userManager.Users.Any() == false)
                {
                    var user = new User
                    {
                        FirstName = "Admin",
                        UserName = "admin@gmail.com",
                        Email = "admin@gmail.com",
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    // Create Admin role if it doesn't exist
                    if ((await roleManager.RoleExistsAsync(Roles.Admin)) == false)
                    {
                        logger.LogInformation("Admin role is creating");
                        var roleResult = await roleManager.CreateAsync(new IdentityRole(Roles.Admin));

                        if (roleResult.Succeeded == false)
                        {
                            var roleErros = roleResult.Errors.Select(e => e.Description);
                            logger.LogError($"Failed to create admin role. Errors : {string.Join(",", roleErros)}");

                            return;
                        }
                        logger.LogInformation("Admin role is created");
                    }

                    // Attempt to create admin user
                    var createUserResult = await userManager.CreateAsync(user: user, password: "Admin@123");

                    // Validate user creation
                    if (createUserResult.Succeeded == false)
                    {
                        var errors = createUserResult.Errors.Select(e => e.Description);
                        logger.LogError(
                            $"Failed to create admin user. Errors: {string.Join(", ", errors)}"
                        );
                        return;
                    }

                    // adding role to user
                    var addUserToRoleResult = await userManager.AddToRoleAsync(user: user, role: Roles.Admin);

                    if (addUserToRoleResult.Succeeded == false)
                    {
                        var errors = addUserToRoleResult.Errors.Select(e => e.Description);
                        logger.LogError($"Failed to add admin role to user. Errors : {string.Join(",", errors)}");
                    }
                    logger.LogInformation("Admin user is created");
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex.Message);
            }

        }
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "Admin", "User", "Manager", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        public static async Task appsetting_SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var flatSettings = new Dictionary<string, string>();
            FlattenConfiguration(configuration, flatSettings);

            var allowedPrefixes = new[]
            {
                    "JwtSettings",
                    "StripeSettings",
                    "PayPalSettings",
                    "OpenAI",
                    "HuggingFace",
                    "EmailSettings"
            };

            var existingKeys = new HashSet<string>(await dbContext.AppSettings.Select(x => x.Key).ToListAsync());

            foreach (var kvp in flatSettings)
            {
                // Include only if key starts with an allowed prefix
                if (allowedPrefixes.Any(prefix => kvp.Key.StartsWith(prefix)) && !existingKeys.Contains(kvp.Key))
                {
                    dbContext.AppSettings.Add(new AppSetting
                    {
                        Key = kvp.Key,
                        Value = kvp.Value
                    });
                }
            }

            await dbContext.SaveChangesAsync();
        }


        // Helper method to recursively flatten IConfiguration into dictionary
        private static void FlattenConfiguration(IConfiguration config, Dictionary<string, string> dict, string parentKey = "")
        {
            foreach (var section in config.GetChildren())
            {
                var key = string.IsNullOrEmpty(parentKey) ? section.Key : $"{parentKey}:{section.Key}";
                if (section.GetChildren().Any())
                {
                    FlattenConfiguration(section, dict, key);
                }
                else
                {
                    dict[key] = section.Value ?? "";
                }
            }
        }
    }
}
