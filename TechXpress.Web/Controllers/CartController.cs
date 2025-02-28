using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Controllers
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
            var cartItems = _cartService.GetCart();
            return View(cartItems);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _productService.GetProductById(productId);
            if (product == null) return NotFound();

            var item = new OrderDetailDTO
            {
                ProductId = product.Id,
                Quantity = quantity,
                Price = product.Price
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
