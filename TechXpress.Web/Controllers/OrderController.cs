using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
