using Microsoft.AspNetCore.Mvc;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace TechXpress.Web.Components
{
    public class CartSummaryViewComponent : ViewComponent
    {
        private readonly ICartService _cartService;

        public CartSummaryViewComponent(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cartItems = _cartService.GetCart();
            var cartViewModel = new CartViewModel
            {
                Items = cartItems,
            };

            return View(cartViewModel);
        }
    }
}
