using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TechXpress.Data.Model;
using TechXpress.Services;
using TechXpress.Services.Base;

namespace TechXpress.Web.Controllers
{
    public class ProductController : Controller
    {

        private readonly IProductService _productService;
        private readonly IWishlistService _wishlistService;
        private readonly UserManager<User> _userManager;
        public ProductController(IProductService productService, IWishlistService wishlistService, UserManager<User> userManager)
        {
            _productService = productService;
            _wishlistService = wishlistService;
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
        public async Task<IActionResult> Details(int id)
        {
           
            var product = await _productService.GetProductDetailsAsync(id);

            if (product == null)
                return NotFound();

            ViewData["PageTitle"] = product.Name;
            ViewData["BreadcrumbPath"] = new List<(string, string)>
            {
                ("/", "Home"),
                ("/Products", "Product"),
                ($"/Products/Details/{id}", product.Name)
            };
            var user = await _userManager.GetUserAsync(User);
            /*if (user!=null)
            {
                var wishlist = await _wishlistService.GetWishlistAsync(user.Id);

                product.IsInWishlist = await _wishlistService.IsInWishlistAsync(user.Id, product.Id);
            }*/
            return View(product);
        }
    }
}
