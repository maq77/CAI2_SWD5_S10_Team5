using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Areas.Customer.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
