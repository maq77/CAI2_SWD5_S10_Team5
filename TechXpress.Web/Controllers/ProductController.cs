using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechXpress.Data.Model;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProducts();
            return View(products);
        }
        // Create Product Form
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllCategories();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductDTO model, List<IFormFile>? images)
        {
            if (!ModelState.IsValid) return View(model);

            await _productService.AddProduct(model, images);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound();
            return View(product);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound();
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(ProductDTO product)
        {
            if (!ModelState.IsValid) return View(product);

            //await _productService.UpdateProduct(product);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteProduct(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
