using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using TechXpress.Data.Model;
using TechXpress.Services;
using TechXpress.Services.Base;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Policy = "Auth")]
    public class WishlistController : Controller
    {
        private readonly IWishlistService _wishListItemService;
        private readonly UserManager<User> _userManager;

        public WishlistController(IWishlistService wishListItemService, UserManager<User> userManager)
        {
            _wishListItemService = wishListItemService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            //var userId = User.Identity.Name; // Or use a UserManager
            var user = await _userManager.GetUserAsync(User);

            if (user==null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var result = await _wishListItemService.AddToWishlistAsync(user.Id, productId);
            if (result)
            {
                return Json(new { success = true, message = "Added to wishlist!" });
            }
            return Json(new { success = false, message = "Product is already in your wishlist." });
        }


        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            //var userId = User.Identity.Name;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }
            var result = await _wishListItemService.RemoveFromWishlistAsync(user.Id, productId);
            if (result)
                return Json(new { success = true, message = "Product removed from wishlist." });

            return Json(new { success = false, message = "Failed to remove product from wishlist." });
        }

        public async Task<IActionResult> Index()
        {
            SetPageMeta();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }
            var wishlist = await _wishListItemService.GetWishlistAsync(user.Id);
            return View(wishlist);
        }
        [HttpGet]
        public async Task<IActionResult> GetWishlistCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }
            var wishlistItems = await _wishListItemService.GetWishlistAsync(user.Id);
            int count = wishlistItems.Count(); // Get the count from the IEnumerable result
            return Json(new { count });
        }
        [HttpGet]
        public async Task<IActionResult> IsinWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }
            var isInWishlist = await _wishListItemService.IsInWishlistAsync(user.Id, productId);
            if (isInWishlist)
                return Json(new { success = true, message = "Product is in wishlist." });
            return Json(new { success = false, message = "Product is not in wishlist." });
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
