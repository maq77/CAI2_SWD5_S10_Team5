using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using TechXpress.Data;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories;
using TechXpress.Data.Repositories.Base;
using TechXpress.Services;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using ProductService = TechXpress.Services.ProductService;
using ReviewService = TechXpress.Services.ReviewService;
using TokenService = TechXpress.Services.TokenService;

namespace TechXpress.Web.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database Configuration
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MyCon"),
                b => b.MigrationsAssembly("TechXpress.Data")));

            // Identity Configuration
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // Register Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductRepo, ProductRepo>();
            services.AddScoped<IOrderRepo, OrderRepo>();

            // Register Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductImageService, ProductImageService>();
            services.AddScoped<IUserImageService, UserImageService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IWishlistService, WishListItemService>();
            services.AddScoped<ITokenRepo, TokenRepo>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IAnalyticsService, AnalyticsService>();
            //services.AddScoped<ITokenService, TokenService>();

            // Session Configuration
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddSingleton<IStripeClient>(provider =>
            {
                var stripeSettings = provider.GetRequiredService<IOptions<StripeSettings>>().Value;
                return new StripeClient(stripeSettings.SecretKey); // secret key
            });

            services.Configure<OpenAISettings>(
                                    configuration.GetSection("OpenAI"));
            services.Configure<HuggingFaceSettings>(
                                    configuration.GetSection("HuggingFace"));





            // Gatway Configuration
            services.AddHttpClient(); // <--- This is required for IHttpClientFactory
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPaymentGateway, PayPalGateway>();
            services.AddScoped<IPaymentGateway, StripeGateway>();
            services.AddScoped<IPaymentGateway, CashGateway>();
            services.AddScoped<IEmailServer, EmailServer>();


            return services;
        }
    }
}
