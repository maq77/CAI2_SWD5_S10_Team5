using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services;
using TechXpress.Services.Base;
using TechXpress.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Application Services
builder.Services.AddApplicationServices(builder.Configuration);

// Add MVC Controllers and Views
builder.Services.AddControllersWithViews();

// Logging Configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


var app = builder.Build();

//Seed Data
//await SeedData.Initialize(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Configure Routing with Areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
