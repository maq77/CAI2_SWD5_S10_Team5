using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using X.PagedList.Extensions;

namespace TechXpress.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductImageService _productImageService;
        private readonly IMemoryCache _cache;

        private const string ProductCacheKey = "ProductList";
        private const string CategoryCacheKey = "CategoryList";

        public ProductController(IProductService productService, ICategoryService categoryService, IProductImageService productImageService, IMemoryCache cache)
        {
            _productService = productService;
            _categoryService = categoryService;
            _productImageService = productImageService;
            _cache = cache;
        }

        // ✅ Index - Show Filtered Products
        public async Task<IActionResult> Index(int? categoryId, string? searchTerm, string? sortOrder, int page = 1, int pageSize = 10)
        {
            await LoadCategoriesToViewBag(categoryId);
            var products = await GetFilteredProducts(categoryId, searchTerm, sortOrder);
            var paginatedProducts = products.ToPagedList(page, pageSize);

            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearch = searchTerm;

            return View(paginatedProducts);
        }

        // ✅ Search - Return Filtered Products as Partial View (for AJAX requests)
        public async Task<IActionResult> Search(string? searchTerm, int? categoryId, string? sortOrder, int pageNumber = 1, int pageSize = 10)
        {
            await LoadCategoriesToViewBag(categoryId);
            var products = await GetFilteredProducts(categoryId, searchTerm, sortOrder);
            var paginatedProducts = products.ToPagedList(pageNumber, pageSize);
            return PartialView("_ProductList", paginatedProducts);
        }

        // ✅ Load More Products (for pagination & infinite scrolling)
        public async Task<IActionResult> LoadMoreProducts(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, int? categoryId = null, string? sortOrder = null)
        {
            await LoadCategoriesToViewBag(categoryId);
            var products = await GetFilteredProducts(categoryId, searchTerm, sortOrder);
            var paginatedProducts = products.ToPagedList(pageNumber, pageSize);
            return PartialView("_ProductList", paginatedProducts);
        }

        // ✅ Create Product (Form)
        public async Task<IActionResult> Create()
        {
            await LoadCategoriesToViewBag();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductDTO model, List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesToViewBag();
                return View(model);
            }

            await _productService.AddProduct(model, images);
            _cache.Remove(ProductCacheKey); // ✅ Clear cache after adding a product

            return RedirectToAction(nameof(Index));
        }

        // ✅ Product Details
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductById(id);
            return product == null ? NotFound() : View(product);
        }

        // ✅ Edit Product (Form)
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound();

            await LoadCategoriesToViewBag(product.CategoryId);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductDTO model, List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesToViewBag(model.CategoryId);
                return View(model);
            }

            if (images != null && images.Count > 0)
            {
                await _productImageService.UploadImages(model.Id, images);
            }

            var result = await _productService.UpdateProduct(model);
            if (!result)
            {
                ModelState.AddModelError("", "Failed to update product.");
                await LoadCategoriesToViewBag(model.CategoryId);
                return View(model);
            }

            _cache.Remove(ProductCacheKey); // ✅ Clear cache after product update
            return RedirectToAction(nameof(Index));
        }

        // ✅ Delete Product (Confirmation Page)
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductById(id);
            return product == null ? NotFound() : View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteProduct(id);
            _cache.Remove(ProductCacheKey); // ✅ Clear cache after deletion
            return RedirectToAction(nameof(Index));
        }

        // ✅ Load Categories with Caching
        private async Task LoadCategoriesToViewBag(int? selectedCategoryId = null)
        {
            if (!_cache.TryGetValue(CategoryCacheKey, out IEnumerable<CategoryDTO>? categories))
            {
                categories = await _categoryService.GetAllCategories();
                _cache.Set(CategoryCacheKey, categories, TimeSpan.FromMinutes(30));
            }

            // ✅ Store as List<CategoryDTO> Instead of SelectList
            ViewBag.Categories = categories;
        }

        // ✅ Get Cached Products
        private async Task<IEnumerable<ProductDTO>> GetCachedProducts()
        {
            if (!_cache.TryGetValue(ProductCacheKey, out IEnumerable<ProductDTO>? products))
            {
                products = await _productService.GetAllProducts();
                _cache.Set(ProductCacheKey, products, TimeSpan.FromMinutes(10));
            }
            return products;
        }

        // ✅ Filter Products
        private async Task<IEnumerable<ProductDTO>> GetFilteredProducts(int? categoryId, string? searchTerm, string? sortOrder)
        {
            var products = await GetCachedProducts();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            return sortOrder switch
            {
                "name_desc" => products.OrderByDescending(p => p.Name),
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "newest" => products.OrderByDescending(p => p.Id),
                "oldest" => products.OrderBy(p => p.Id),
                _ => products.OrderBy(p => p.Name)
            };
        }

        // ✅ Paginate Products
        private IEnumerable<ProductDTO> PaginateProducts(IEnumerable<ProductDTO> products, int pageNumber, int pageSize)
        {
            return products.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }

        public async Task<IActionResult> GetPagination(int page = 1, int pageSize = 5)
        {
            var products = await _productService.GetAllProducts();
            var pagedProducts = products.ToPagedList(page, pageSize);
            return PartialView("_Pagination", pagedProducts);
        }
    }
}
