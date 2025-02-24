using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services;
using TechXpress.Services.Base;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyCon"), 
    b => b.MigrationsAssembly("TechXpress.Data")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
/*
async Task SeedAdminUserAsync(IServiceProvider services)
{
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@techxpress.com";
    string adminPassword = "Admin@123";

    //  Ensure Admin Role Exists
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    //  Ensure Admin User Exists
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
            Console.WriteLine(" Admin user created successfully!");
        }
        else
        {
            Console.WriteLine("Failed to create admin user.");
        }
    }
    else
    {
        Console.WriteLine("✅ Admin user already exists.");
    }
}
*/