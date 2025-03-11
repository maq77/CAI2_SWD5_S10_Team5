using Microsoft.AspNetCore.Mvc;

namespace TechXpress.Web.Controllers.Customer
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
