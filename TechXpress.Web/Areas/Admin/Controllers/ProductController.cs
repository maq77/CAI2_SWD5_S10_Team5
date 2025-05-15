using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using TechXpress.Services.DTOs.ViewModels;
using X.PagedList.Extensions;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductImageService _productImageService;

        public ProductController(
            IProductService productService,
            ICategoryService categoryService,
            IProductImageService productImageService)

        {
            _productService = productService;
            _categoryService = categoryService;
            _productImageService = productImageService;
        }

        // GET: Product/Index
        public async Task<IActionResult> Index(int? categoryId, string? searchTerm, string? sortOrder, int page = 1, int pageSize = 10)
        {
            await LoadCategoriesToViewBag(categoryId);

            var products = await GetFilteredProducts(categoryId, searchTerm, sortOrder);
            var paginatedProducts = products.ToPagedList(page, pageSize);

            SetViewBagParameters(categoryId, searchTerm, sortOrder);

            return View(paginatedProducts);
        }

        // GET: Product/Search (for AJAX requests)
        public async Task<IActionResult> Search(string? searchTerm, int? categoryId, string? sortOrder, int page = 1, int pageSize = 10)
        {
            var products = await GetFilteredProducts(categoryId, searchTerm, sortOrder);
            var paginatedProducts = products.ToPagedList(page, pageSize);

            return PartialView("_ProductList", paginatedProducts);
        }

        // GET: Product/LoadMoreProducts (for infinite scrolling)
        public async Task<IActionResult> LoadMoreProducts(int page = 1, int pageSize = 10, string? searchTerm = null, int? categoryId = null, string? sortOrder = null)
        {
            var products = await GetFilteredProducts(categoryId, searchTerm, sortOrder);
            var paginatedProducts = products.ToPagedList(page, pageSize);

            return PartialView("_ProductList", paginatedProducts);
        }

        // GET: Product/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ProductCreateViewModel
            {
                Categories = (await _categoryService.GetAllCategories())
                            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                            .ToList()
            };

            return View(model);
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var productDTO = new ProductDTO
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                CategoryId = model.CategoryId,
                StockQunatity = model.StockQuantity

            };

            await _productService.AddProduct(productDTO, model.Images);
            return Json(new { success = true, message = "Product added successfully!" });
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["PageTitle"] = product.Name;
            ViewData["BreadcrumbPath"] = new List<(string, string)>
            {
                ("/", "Home"),
                ("/Products", "Product"),
                ($"/Products/Details/{id}", product.Name)
            };

            return View(product);
        }

        // GET: Product/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                StockQuantity = product.StockQunatity,
                ExistingImages = await _productImageService.GetImagesByProductId(product.Id) ?? new List<ProductImageDTO>(),
                Categories = await GetCategorySelectList()
            };

            return View(model);
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEditViewModel model, List<int>? deleteImageIds, List<IFormFile>? newImages)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await GetCategorySelectList();
                return View(model);
            }

            var product = await _productService.GetProductById(model.Id);
            if (product == null)
            {
                return NotFound();
            }

            // Update product properties
            product.Name = model.Name;
            product.Description = model.Description;
            product.Price = model.Price;
            product.StockQunatity = model.StockQuantity;
            if (model.CategoryId.HasValue)
            {
                product.CategoryId = model.CategoryId.Value;
            }

            // Handle image deletions
            if (deleteImageIds?.Count > 0)
            {
                foreach (var imageId in deleteImageIds)
                {
                    await _productImageService.DeleteImage(imageId);
                }
            }

            // Handle new image uploads
            if (newImages?.Count > 0)
            {
                await _productImageService.UploadImages(product.Id, newImages);
            }

            await _productService.UpdateProduct(product);
            return RedirectToAction(nameof(Index));
        }

        // POST: Product/RemoveImage/5
        [HttpPost]
        public async Task<IActionResult> RemoveImage(int imageId)
        {
            var success = await _productImageService.DeleteImage(imageId);
            return success ? Ok() : BadRequest("Failed to delete image.");
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteProduct(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Product/GetPagination
        public async Task<IActionResult> GetPagination(int page = 1, int pageSize = 5)
        {
            var products = await _productService.GetAllProducts();
            var pagedProducts = products.ToPagedList(page, pageSize);

            return PartialView("_Pagination", pagedProducts);
        }

        #region Private Helper Methods

        private async Task<List<SelectListItem>> GetCategorySelectList()
        {
            var categories = await _categoryService.GetAllCategories();
            return categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList();
        }

        private async Task LoadCategoriesToViewBag(int? selectedCategoryId = null)
        {
            var categories = await _categoryService.GetAllCategories();
            ViewBag.Categories = categories;
        }

        private async Task<IEnumerable<ProductDTO>> GetFilteredProducts(int? categoryId, string? searchTerm, string? sortOrder)
        {
            var products = await _productService.GetAllProducts();

            // Filter by category
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter by search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchTerm));
            }

            // Apply sorting
            return ApplySorting(products, sortOrder);
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