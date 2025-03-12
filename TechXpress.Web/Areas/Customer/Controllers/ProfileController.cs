using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
