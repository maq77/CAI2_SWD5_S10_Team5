using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
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
        public async Task<IActionResult> Index(int? categoryId, string? searchQuery, string? sortOrder, int page = 1, int pageSize = 9)
        {
            SetPageMeta();
            var shopData = new ShopPageDTO();

            var categories = await LoadCategories();
            var filteredProducts = await GetFilteredProducts(categoryId, searchQuery, sortOrder, page, pageSize);

            shopData.Products = filteredProducts;
            shopData.Categories = categories;
            shopData.SearchQuery = searchQuery;
            shopData.SelectedCategoryId = categoryId;

            SetViewBagParameters(categoryId, searchQuery, sortOrder);

            // Check if this is an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", filteredProducts);
            }

            return View(shopData);
        }

        // GET: Shop/Search (for AJAX requests)
        public async Task<IActionResult> Search(string? searchQuery, int? categoryId, string? sortOrder, int page = 1, int pageSize = 9)
        {
            var filtered_products = await GetFilteredProducts(categoryId, searchQuery, sortOrder, page, pageSize);
            return PartialView("_ProductList", filtered_products);
        }

        // GET: Shop/GetFilteredProducts (for AJAX requests)
        public async Task<IActionResult> GetFilteredProducts(string? searchQuery, int? categoryId, string? sortOrder, int page = 1)
        {
            var pageSize = 9; // Consistent page size
            var filtered_products = await GetFilteredProducts(categoryId, searchQuery, sortOrder, page, pageSize);
            return PartialView("_ProductList", filtered_products);
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
      
        //

        //  Fetch Products with Filtering & Pagination
        private async Task<IPagedList<ProductDTO>> GetFilteredProducts(int? categoryId, string? searchQuery, string? sortOrder, int page, int pageSize)
        {
            if (!_cache.TryGetValue(ProductCacheKey, out IEnumerable<ProductDTO>? products))
            {
                products = await _productService.GetAllProducts();
                _cache.Set(ProductCacheKey, products, TimeSpan.FromMinutes(1));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchQuery) ||
                                              (p.Description != null && p.Description.ToLower().Contains(searchQuery)));
            }

            return ApplySorting(products, sortOrder).ToPagedList(page, pageSize);
        }

        private async Task<IEnumerable<CategoryDTO>> LoadCategories()
        {
            if (!_cache.TryGetValue(CategoryCacheKey, out IEnumerable<CategoryDTO>? categories))
            {
                categories = await _categoryService.GetAllCategories();
                _cache.Set(CategoryCacheKey, categories, TimeSpan.FromMinutes(1));
            }
            return categories;
        }

        private IEnumerable<ProductDTO> ApplySorting(IEnumerable<ProductDTO> products, string? sortOrder)
        {
            return sortOrder switch
            {
                "name_desc" => products.OrderByDescending(p => p.Name),
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "newest" => products.OrderByDescending(p => p.Id),
                "oldest" => products.OrderBy(p => p.Id),
                _ => products.OrderBy(p => p.Name) // Default sort by name ascending
            };
        }

        private void SetViewBagParameters(int? categoryId, string? searchTerm, string? sortOrder)
        {
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearch = searchTerm;
        }
        #endregion
    }
}