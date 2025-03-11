using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Controllers.Customer
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
