using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechXpress.Data.Model;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategories();
            return View(categories);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryDTO model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data!" });

            await _categoryService.AddCategory(model);
            return Json(new { success = true });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryById(id);
            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryDTO model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data!" });

            await _categoryService.UpdateCategory(model);
            return Json(new { success = true });
        }

        /*public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteCategory(id);
            return RedirectToAction(nameof(Index));
        }*/
        /*public async Task<IActionResult> Delete(int id)
        {
            var cat = await _categoryService.GetCategoryById(id);
            if (cat == null) return NotFound($"No Category with {id} !");
            return RedirectToAction(nameof(Index));
        }*/

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }

            await _categoryService.DeleteCategory(id);
            return Json(new { success = true });
        }
    }
}
