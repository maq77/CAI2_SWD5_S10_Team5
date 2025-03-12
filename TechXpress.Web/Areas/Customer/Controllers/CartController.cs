using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using TechXpress.Services.DTOs.ViewModels;

namespace TechXpress.Web.Areas.Customer.Controllers
{
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


        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null) return NotFound();

            var item = new OrderDetailDTO
            {
                ProductId = product.Id,
                Quantity = quantity,
                Price = product.Price,
                Product = product
            };

            _cartService.AddToCart(item);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateCart(List<OrderDetailDTO> updatedCart)
        {
            _cartService.UpdateCart(updatedCart);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.RemoveFromCart(productId);
            return RedirectToAction("Index");
        }

        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return RedirectToAction("Index");
        }
    }
}
