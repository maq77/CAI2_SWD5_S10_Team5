using Microsoft.AspNetCore.Mvc;
using TechXpress.Data.Model;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Controllers
{
    public class ProductController : Controller
    {
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        private readonly IProductService _productService;
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProducts();
            return View(products);
        }
        // Create Product Form
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductDTO model, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(model);

            await _productService.AddProductAsync(model, image);
            return RedirectToAction(nameof(Index));
        }
    }
}
