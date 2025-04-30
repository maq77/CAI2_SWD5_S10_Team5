using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Services;
using TechXpress.Services.Base;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishListItemService;

        public WishlistController(IWishlistService wishListItemService)
        {
            _wishListItemService = wishListItemService;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = User.Identity.Name; // Or use a UserManager if using Identity

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var result = await _wishListItemService.AddToWishlistAsync(userId, productId);
            if (result)
            {
                return Json(new { success = true, message = "Added to wishlist!" });
            }
            return Json(new { success = false, message = "Product is already in your wishlist." });
        }


        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = User.Identity.Name;
            var result = await _wishListItemService.RemoveFromWishlistAsync(userId, productId);
            if (result)
                return Json(new { success = true, message = "Product removed from wishlist." });

            return Json(new { success = false, message = "Failed to remove product from wishlist." });
        }

        public async Task<IActionResult> Index()
        {
            SetPageMeta();
            var userId = User.Identity.Name; // Replace with actual user ID
            var wishlist = await _wishListItemService.GetWishlistAsync(userId);
            return View(wishlist);
        }
        [HttpGet]
        public async Task<IActionResult> GetWishlistCount()
        {
            var userId = User.Identity.Name; // Ensure this retrieves the correct user ID
            var wishlistItems = await _wishListItemService.GetWishlistAsync(userId);
            int count = wishlistItems.Count(); // Get the count from the IEnumerable result
            return Json(new { count });
        }
        #region Private Helper Methods

        //
        protected void SetPageMeta()
        {
            var controller = ControllerContext.ActionDescriptor.ControllerName;
            var action = ControllerContext.ActionDescriptor.ActionName;

            ViewData["PageTitle"] = FormatTitle(action);
            ViewData["BreadcrumbPath"] = new List<(string, string)>
        {
            ("/", "Home"),
            ($"/{controller}", FormatTitle(controller))
        };
        }

        private string FormatTitle(string text) =>
            System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.Replace("_", " ").ToLower());
        #endregion
    }
}
