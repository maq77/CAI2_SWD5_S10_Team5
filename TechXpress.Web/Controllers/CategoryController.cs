using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
