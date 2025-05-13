using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using X.PagedList;
using X.PagedList.Extensions;

namespace TechXpress.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductImageService _productImageService;
        private readonly IMemoryCache _cache;

        private const string FeaturedProductCacheKey = "FeaturedProducts";
        private const string CategoryCacheKey = "HomeCategories";

        public HomeController(IProductService productService, ICategoryService categoryService, IProductImageService productImageService, IMemoryCache cache)
        {
            _productService = productService;
            _categoryService = categoryService;
            _productImageService = productImageService;
            _cache = cache;
        }

        //  Load Home Page with Popular Products & Categories
        public async Task<IActionResult> Index(int? categoryId, int page = 1, int pageSize = 6)
        {
            SetPageMeta();
            var homePageData = new HomePageDTO();

            if (!_cache.TryGetValue(FeaturedProductCacheKey, out IEnumerable<ProductDTO>? featuredProducts))
            {
                featuredProducts = await _productService.GetPopularProducts(); //  Fetch Bestselling Products
                _cache.Set(FeaturedProductCacheKey, featuredProducts, TimeSpan.FromMinutes(30));
            }

            if (!_cache.TryGetValue(CategoryCacheKey, out IEnumerable<CategoryDTO>? categories))
            {
                categories = await _categoryService.GetAllCategories();
                _cache.Set(CategoryCacheKey, categories, TimeSpan.FromMinutes(30));
            }

            var filteredProducts = await GetFilteredProducts(categoryId, page, pageSize);
            homePageData.FeaturedProducts = featuredProducts ?? await _productService.GetAllProducts();
            homePageData.Categories = categories;
            homePageData.PaginatedProducts = filteredProducts;

            ViewBag.CurrentCategory = categoryId;
            return View(homePageData);
        }
        /// 
        /// 
        private async Task LoadCategoriesToViewBag(int? selectedCategoryId = null)
        {
            if (!_cache.TryGetValue(CategoryCacheKey, out IEnumerable<CategoryDTO>? categories))
            {
                categories = await _categoryService.GetAllCategories();
                _cache.Set(CategoryCacheKey, categories, TimeSpan.FromMinutes(30));
            }

            //  Store as List<CategoryDTO> Instead of SelectList
            ViewBag.Categories = categories;
        }
        private async Task<IEnumerable<ProductDTO>> GetCachedProducts()
        {
            if (!_cache.TryGetValue(FeaturedProductCacheKey, out IEnumerable<ProductDTO>? products))
            {
                products = await _productService.GetAllProducts();
                _cache.Set(FeaturedProductCacheKey, products, TimeSpan.FromMinutes(10));
            }
            return products;
        }

        //  Load More Products via AJAX (Pagination)
        public async Task<IActionResult> LoadMoreProducts(int page = 1, int pageSize = 6, int? categoryId = null)
        {
            var products = await GetFilteredProducts(categoryId, page, pageSize);
            return PartialView("_ProductList", products);
        }
        public  IActionResult Error(string error="", string message="")
        {
            return View();
        }

        //  Fetch Products with Filtering & Pagination
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
        private async Task<IPagedList<ProductDTO>> GetFilteredProducts(int? categoryId, int page, int pageSize)
        {
            var products = await GetCachedProducts();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            return products.OrderByDescending(p => p.Id).ToPagedList(page, pageSize);
        }
        #endregion
    }
}
