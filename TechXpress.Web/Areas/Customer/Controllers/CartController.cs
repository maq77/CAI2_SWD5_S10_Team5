using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using TechXpress.Services.DTOs.ViewModels;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        public IActionResult Index()
        {
            ViewData["PageTitle"] = "Shopping Cart";
            ViewData["BreadcrumbPath"] = new List<(string, string)>
            {
                  ("/", "Home"),
                  ("/Cart", "Shopping Cart")
             };
            var cartItems = _cartService.GetCart();
            var cartViewModel = new CartViewModel
            {
                Items = cartItems ?? new List<OrderDetailDTO>() // Ensure it's never null
            };

            return View(cartViewModel);
        }
        public IActionResult Summary()
        {
            var cartItems = _cartService.GetCart();
            var cartViewModel = new CartViewModel { Items = cartItems };
            return PartialView("_CartSummary", cartViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            
            int count = _cartService.GetCartItemCount();
            return Json(new { count });
        }

        public IActionResult GetCartSummary()
        {
            var cart = _cartService.GetCart();
            var viewModel = new CartViewModel
            {
                Items = cart
            };
            return PartialView("_CartSummary", viewModel);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity = 1)
        {
            _cartService.UpdateQuantity(productId, quantity);
            var item = _cartService.GetCartItem(productId);
            if (item == null)
            {
                return Json(new
                {
                    success = false,
                    itemTotal = 0.ToString("C"),
                    cartTotal = 0.ToString("C")
                });
            }
            double itemTotal = ( item.Quantity * item.Price );
            double cartTotal = _cartService.GetCartTotal();
            return Json(new
            {
                success = true,
                itemTotal = itemTotal.ToString("C"),
                cartTotal = cartTotal.ToString("C")
            });
        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity=1)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null) { return Json(new { success = false, message = "Product Not Found" }); }
            var item = new OrderDetailDTO
            {
                ProductId = product.Id,
                Quantity = quantity,
                Price = product.Price,
                Product = product
            };

            var res = await _cartService.AddToCart(item);
            if (res)
            {
                return Json(new { success = true, message = "Added to cart!" });
            }
            else
            {
                return Json(new { success = false, message = "Error While Adding To cart" });
            }

        }

        //[HttpPost]
        //public IActionResult UpdateCart(List<OrderDetailDTO> updatedCart)
        //{
        //    _cartService.UpdateCart(updatedCart);
        //    return Json(new { success = true, message = "Quantity updated" });
        //}


        public IActionResult RemoveFromCart(int productId)
        {
            var res = _cartService.RemoveFromCart(productId);
            if (res)
            {
                return Json(new { success = true, message = "Product removed from cart." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to remove product from cart." });
            }
        }

        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return RedirectToAction("Index");
        }
    }
}