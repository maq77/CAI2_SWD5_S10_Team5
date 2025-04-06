using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using X.PagedList;
using X.PagedList.Extensions;

namespace TechXpress.Web.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IMemoryCache _cache;
        private const string ProductCacheKey = "ShopProducts";
        private const string CategoryCacheKey = "ShopCategories";

        public ShopController(IProductService productService, ICategoryService categoryService, IMemoryCache cache)
        {
            _productService = productService;
            _categoryService = categoryService;
            _cache = cache;
        }

        //  Load Shop Page with Filtering & Pagination
        public async Task<IActionResult> Index(int? categoryId, string searchQuery, int page = 1, int pageSize = 9)
        {
            var shopData = new ShopPageDTO();

            if (!_cache.TryGetValue(CategoryCacheKey, out IEnumerable<CategoryDTO>? categories))
            {
                categories = await _categoryService.GetAllCategories();
                _cache.Set(CategoryCacheKey, categories, TimeSpan.FromMinutes(30));
            }

            var filteredProducts = await GetFilteredProducts(categoryId, searchQuery, page, pageSize);

            shopData.Products = filteredProducts;
            shopData.Categories = categories;
            shopData.SearchQuery = searchQuery;
            shopData.SelectedCategoryId = categoryId;

            return View(shopData);
        }

        //  Fetch Products with Filtering & Pagination
        private async Task<IPagedList<ProductDTO>> GetFilteredProducts(int? categoryId, string searchQuery, int page, int pageSize)
        {
            if (!_cache.TryGetValue(ProductCacheKey, out IEnumerable<ProductDTO>? products))
            {
                products = await _productService.GetAllProducts();
                _cache.Set(ProductCacheKey, products, TimeSpan.FromMinutes(30));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(p => p.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));
            }

            return products.OrderByDescending(p => p.Id).ToPagedList(page, pageSize);
        }
    }
}
