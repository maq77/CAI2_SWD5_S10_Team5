using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
