using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Controllers.Admin
{
    public class OrderManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
