using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Stripe;
using TechXpress.Data.Model;
using TechXpress.Services;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs.ViewModels;

namespace TechXpress.Web.Controllers
{
    public class ProductController : Controller
    {

        private readonly IProductService _productService;
        private readonly IWishlistService _wishlistService;
        private readonly IReviewService _reviewService;
        private readonly UserManager<User> _userManager;
        public ProductController(IProductService productService, IWishlistService wishlistService, IReviewService reviewService, UserManager<User> userManager)
        {
            _productService = productService;
            _wishlistService = wishlistService;
            _reviewService = reviewService;
            _userManager = userManager;
        }
        //public IActionResult Index()
        //{
        //    ViewData["PageTitle"] = "PRDOUCT";
        //    ViewData["BreadcrumbPath"] = new List<(string, string)>
        //    {
        //          ("/", "Home"),
        //          ("/Product", "Product")
        //     };
        //    return View();
        //}
        public async Task<IActionResult> Details(int id, bool showReviews = false)
        {
            try
            {
                var product = await _productService.GetProductDetailsAsync(id);

                if (product == null)
                    return NotFound();
                SetPageMeta(product.Name, id);
                var viewModel = new ProductDetailsViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    CategoryName = product.CategoryName,
                    StockQuantity = product.StockQuantity,
                    Images = product.Images
                };
                // Get reviews for this product
                var reviews = await _reviewService.GetReviewsByProductIdAsync(id);
                viewModel.Reviews = reviews;
                viewModel.ReviewCount = reviews.Count;

                // Calculate average rating
                if (reviews.Any())
                {
                    viewModel.AverageRating = await _reviewService.GetAverageRatingByProductIdAsync(id);
                }
                else
                {
                    viewModel.AverageRating = 0;
                }
                var user = await _userManager.GetUserAsync(User);

                if (user!=null)
                {
                    product.IsInWishlist = await _wishlistService.IsInWishlistAsync(user.Id, product.Id);
                }
                // Show reviews tab if requested
                if (showReviews)
                {
                    ViewBag.ActiveTab = "reviews";
                }
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                return RedirectToAction("Error", "Home");
            }
        }
        protected void SetPageMeta(string productName, int id)
        {

            ViewData["PageTitle"] = productName;
            ViewData["BreadcrumbPath"] = new List<(string, string)>
            {
                ("/", "Home"),
                ("/Products", "Product"),
                ($"/Products/Details/{id}", productName)
            };

        }
    }
}
