using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
