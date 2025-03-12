using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var provider = scope.ServiceProvider;

            var userManager = provider.GetRequiredService<UserManager<User>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

            string adminEmail = "admin@techxpress.com";
            string adminPassword = "Admin@123";

            // Ensure Admin Role Exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Ensure Admin User Exists
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                    Console.WriteLine("Admin user created successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to create admin user.");
                }
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
        }
    }
}
